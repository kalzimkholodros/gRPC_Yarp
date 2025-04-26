using BasketService.Data;
using BasketService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using Shared.Protos;
using Grpc.Net.Client;
using System.Threading.Tasks;

namespace BasketService.Services
{
    public class BasketService : IBasketService
    {
        private readonly BasketDbContext _context;
        private readonly IDistributedCache _cache;
        private readonly ProductGrpc.ProductGrpcClient _productClient;

        public BasketService(BasketDbContext context, IDistributedCache cache, ProductGrpc.ProductGrpcClient productClient)
        {
            _context = context;
            _cache = cache;
            _productClient = productClient;
        }

        public async Task<Basket> GetBasketAsync(string userId)
        {
            var basket = await _context.Baskets
                .Include(b => b.Items)
                .FirstOrDefaultAsync(b => b.UserId == userId);

            if (basket == null)
            {
                basket = new Basket { UserId = userId };
                _context.Baskets.Add(basket);
                await _context.SaveChangesAsync();
            }

            return basket;
        }

        public async Task<Basket> AddItemToBasketAsync(string userId, int productId, int quantity)
        {
            var basket = await GetBasketAsync(userId);
            var product = await _productClient.GetProductAsync(new GetProductRequest { Id = productId });

            if (product == null)
            {
                throw new Exception("Ürün bulunamadı");
            }

            var existingItem = basket.Items.FirstOrDefault(i => i.ProductId == productId);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                basket.Items.Add(new BasketItem
                {
                    ProductId = productId,
                    ProductName = product.Name,
                    Price = (decimal)product.Price,
                    Quantity = quantity
                });
            }

            await _context.SaveChangesAsync();
            await SetBasketInCacheAsync(userId, basket);

            return basket;
        }

        public async Task<Basket> RemoveItemFromBasketAsync(string userId, int productId)
        {
            var basket = await GetBasketAsync(userId);
            var item = basket.Items.FirstOrDefault(i => i.ProductId == productId);

            if (item != null)
            {
                basket.Items.Remove(item);
                await _context.SaveChangesAsync();
                await SetBasketInCacheAsync(userId, basket);
            }

            return basket;
        }

        public async Task ClearBasketAsync(string userId)
        {
            var basket = await GetBasketAsync(userId);
            basket.Items.Clear();
            await _context.SaveChangesAsync();
            await RemoveBasketFromCacheAsync(userId);
        }

        private async Task<Basket?> GetBasketFromCacheAsync(string userId)
        {
            var cachedBasket = await _cache.GetStringAsync($"basket_{userId}");
            if (cachedBasket != null)
            {
                return JsonSerializer.Deserialize<Basket>(cachedBasket);
            }
            return null;
        }

        private async Task SetBasketInCacheAsync(string userId, Basket basket)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
            };

            var serializedBasket = JsonSerializer.Serialize(basket);
            await _cache.SetStringAsync($"basket_{userId}", serializedBasket, options);
        }

        private async Task RemoveBasketFromCacheAsync(string userId)
        {
            await _cache.RemoveAsync($"basket_{userId}");
        }
    }
} 