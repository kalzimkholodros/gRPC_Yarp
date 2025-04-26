using BasketService.Models;
using System.Threading.Tasks;

namespace BasketService.Services
{
    public interface IBasketService
    {
        Task<Basket> GetBasketAsync(string userId);
        Task<Basket> AddItemToBasketAsync(string userId, int productId, int quantity);
        Task<Basket> RemoveItemFromBasketAsync(string userId, int productId);
        Task ClearBasketAsync(string userId);
    }
} 