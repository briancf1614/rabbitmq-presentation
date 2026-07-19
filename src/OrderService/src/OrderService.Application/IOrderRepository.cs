using System.Threading;
using System.Threading.Tasks;
using OrderService.Domain;

namespace OrderService.Application
{
    public interface IOrderRepository
    {
        Task AddAsync(Order order, CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
