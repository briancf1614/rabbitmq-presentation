using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using OrderService.Application;
using System.Net.Http;

namespace OrderService.IntegrationTests
{
    public class OrderEndpointsTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public OrderEndpointsTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task CreateOrder_Should_ReturnCreated_When_Valid()
        {
            // Arrange
            var command = new CreateOrderCommand
            {
                CustomerName = "Integration Test User",
                TotalAmount = 250.50m
            };

            // Act - Send request to the real API (running against Testcontainers)
            var response = await _client.PostAsJsonAsync("/api/v1/orders", command);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            // We could also reach into the DB directly here to verify data was saved
        }
    }
}
