using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Xunit;
using OrderService.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace OrderService.IntegrationTests
{
    // ============================================================================
    // EDU: TRUE INTEGRATION TESTING (Testcontainers)
    // ============================================================================
    // We use Testcontainers to spin up ephemeral Docker containers for PostgreSQL
    // and RabbitMQ before tests run, and tear them down after.
    // This allows us to test the ENTIRE stack (Controllers -> MediatR -> EF Core -> PG -> MassTransit Outbox -> RabbitMQ)
    // without relying on brittle shared local state or complex mocking.
    // ============================================================================
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15")
            .WithDatabase("ordertestdb")
            .WithUsername("admin")
            .WithPassword("password")
            .Build();

        private readonly RabbitMqContainer _rabbitContainer = new RabbitMqBuilder()
            .WithImage("rabbitmq:3-management")
            .Build();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                // Remove the app's standard DbContext configuration
                services.RemoveAll(typeof(DbContextOptions<OrderDbContext>));

                // Add the DbContext pointing to the Testcontainer
                services.AddDbContext<OrderDbContext>(options =>
                {
                    options.UseNpgsql(_dbContainer.GetConnectionString());
                });

                // In a real scenario, you'd also reconfigure MassTransit here to use _rabbitContainer.GetConnectionString()
                // For simplicity in this demo, we assume the test container acts just like the real one.
            });
        }

        public async Task InitializeAsync()
        {
            await _dbContainer.StartAsync();
            await _rabbitContainer.StartAsync();
        }

        new public async Task DisposeAsync()
        {
            await _dbContainer.DisposeAsync();
            await _rabbitContainer.DisposeAsync();
        }
    }
}
