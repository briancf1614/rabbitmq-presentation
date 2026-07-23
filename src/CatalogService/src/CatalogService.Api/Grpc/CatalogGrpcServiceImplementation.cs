using System;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using CatalogService.Api.GraphQL;

namespace CatalogService.Api.Grpc
{
    public class CatalogGrpcServiceImplementation : CatalogGrpcService.CatalogGrpcServiceBase
    {
        private readonly Query _queryRoot;

        public CatalogGrpcServiceImplementation()
        {
            _queryRoot = new Query();
        }

        public override Task<ValidateProductResponse> ValidateProduct(ValidateProductRequest request, ServerCallContext context)
        {
            if (Guid.TryParse(request.ProductId, out var productId))
            {
                var product = _queryRoot.GetProductById(productId);
                if (product != null)
                {
                    return Task.FromResult(new ValidateProductResponse
                    {
                        IsValid = true,
                        ProductName = product.Name,
                        Price = (double)product.Price
                    });
                }
            }

            return Task.FromResult(new ValidateProductResponse { IsValid = false });
        }
    }
}
