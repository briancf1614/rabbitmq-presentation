using System.Threading;
using System.Threading.Tasks;
using OrderService.Application;
using CatalogService.Api.Grpc;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;

namespace OrderService.Infrastructure
{
    public class CatalogGrpcClient : ICatalogGrpcClient
    {
        private readonly CatalogGrpcService.CatalogGrpcServiceClient _client;

        public CatalogGrpcClient(IConfiguration configuration)
        {
            var channel = GrpcChannel.ForAddress(configuration["CatalogService:GrpcUrl"] ?? "http://catalogservice:80");
            _client = new CatalogGrpcService.CatalogGrpcServiceClient(channel);
        }

        public async Task<bool> ValidateProductAsync(string productId, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _client.ValidateProductAsync(new ValidateProductRequest { ProductId = productId }, cancellationToken: cancellationToken);
                return response.IsValid;
            }
            catch
            {
                // In a real app, handle Grpc errors properly. For educational purposes, fail validation.
                return false;
            }
        }
    }
}
