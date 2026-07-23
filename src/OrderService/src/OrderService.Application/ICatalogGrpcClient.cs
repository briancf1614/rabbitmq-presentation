using System.Threading;
using System.Threading.Tasks;

namespace OrderService.Application
{
    public interface ICatalogGrpcClient
    {
        Task<bool> ValidateProductAsync(string productId, CancellationToken cancellationToken = default);
    }
}
