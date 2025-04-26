using Grpc.Core;
using ProductService.Data;
using Shared.Protos;
using Microsoft.EntityFrameworkCore;

namespace ProductService.Services
{
    public class ProductGrpcService : ProductGrpc.ProductGrpcBase
    {
        private readonly ProductDbContext _context;

        public ProductGrpcService(ProductDbContext context)
        {
            _context = context;
        }

        public override async Task<ProductResponse> GetProduct(GetProductRequest request, ServerCallContext context)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == request.Id);

            if (product == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Product not found"));
            }

            return new ProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = Convert.ToDouble(product.Price),
                Stock = product.Stock
            };
        }
    }
} 