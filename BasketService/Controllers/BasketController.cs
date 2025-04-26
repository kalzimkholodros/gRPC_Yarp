using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BasketService.Services;
using BasketService.Models;
using System.Security.Claims;
using System.Threading.Tasks;
using Shared.Protos;

namespace BasketService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BasketController : ControllerBase
    {
        private readonly IBasketService _basketService;
        private readonly ProductGrpc.ProductGrpcClient _productClient;

        public BasketController(IBasketService basketService, ProductGrpc.ProductGrpcClient productClient)
        {
            _basketService = basketService;
            _productClient = productClient;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<Basket>> GetBasket()
        {
            var userId = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var basket = await _basketService.GetBasketAsync(userId);
            return Ok(basket);
        }

        [HttpPost("items")]
        [Authorize]
        public async Task<ActionResult<Basket>> AddItem([FromBody] AddItemRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // Ürün bilgilerini ProductService'den al
            var product = await _productClient.GetProductAsync(new GetProductRequest { Id = request.ProductId });
            if (product == null)
            {
                return BadRequest("Ürün bulunamadı");
            }

            try
            {
                var basket = await _basketService.AddItemToBasketAsync(userId, request.ProductId, request.Quantity);
                return Ok(basket);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("items/{productId}")]
        [Authorize]
        public async Task<ActionResult<Basket>> RemoveItem(int productId)
        {
            var userId = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var basket = await _basketService.RemoveItemFromBasketAsync(userId, productId);
            return Ok(basket);
        }

        [HttpDelete]
        [Authorize]
        public async Task<ActionResult> ClearBasket()
        {
            var userId = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            await _basketService.ClearBasketAsync(userId);
            return NoContent();
        }

        [HttpGet("{userId}")]
        [Authorize]
        public async Task<IActionResult> GetBasket(int userId)
        {
            var basket = await _basketService.GetBasketAsync(userId.ToString());
            if (basket == null)
            {
                return NotFound();
            }

            // Her ürün için ProductService'den detaylı bilgileri al
            foreach (var item in basket.Items)
            {
                var product = await _productClient.GetProductAsync(new GetProductRequest { Id = item.ProductId });
                if (product != null)
                {
                    item.ProductName = product.Name;
                    item.Price = (decimal)product.Price;
                }
            }

            return Ok(basket);
        }
    }

    public class AddItemRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
} 