using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Shared.Protos;
using Grpc.Core;

namespace BasketService.Services
{
    public class ProductGrpcClient
    {
        private readonly ProductGrpc.ProductGrpcClient _client;

        public ProductGrpcClient(GrpcChannel channel)
        {
            _client = new ProductGrpc.ProductGrpcClient(channel);
        }

        public async Task<ProductResponse?> GetProductAsync(int productId)
        {
            try
            {
                var request = new GetProductRequest { Id = productId };
                return await _client.GetProductAsync(request);
            }
            catch (RpcException ex)
            {
                if (ex.StatusCode == StatusCode.NotFound)
                {
                    return null;
                }
                throw;
            }
        }
    }
} 