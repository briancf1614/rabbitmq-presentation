using Dapper;
using MediatR;
using Npgsql;
using Microsoft.Extensions.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace OrderService.Application
{
    // ============================================================================
    // EDU: STRICT CQRS WITH DAPPER
    // ============================================================================
    // While Entity Framework Core is excellent for Writes (Commands) because of
    // change tracking and complex domain modeling, it can add overhead for simple Reads.
    // Here we use Dapper (a micro-ORM) to query the database directly using raw SQL.
    // This is the read-side of CQRS, optimized purely for speed and simplicity.
    // ============================================================================
    public class GetOrderQueryHandler : IRequestHandler<GetOrderQuery, OrderDto>
    {
        private readonly string _connectionString;

        public GetOrderQueryHandler(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? "Host=postgres;Port=5432;Database=orderdb;Username=admin;Password=password";
        }

        public async Task<OrderDto> Handle(GetOrderQuery request, CancellationToken cancellationToken)
        {
            using var connection = new NpgsqlConnection(_connectionString);

            // Raw SQL is extremely fast and maps directly to our flat DTO
            const string sql = @"
                SELECT ""Id"", ""CustomerName"", ""TotalAmount"", ""Status""
                FROM ""Orders""
                WHERE ""Id"" = @OrderId";

            var order = await connection.QuerySingleOrDefaultAsync<OrderDto>(sql, new { request.OrderId });
            return order;
        }
    }
}
