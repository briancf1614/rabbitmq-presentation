using FluentAssertions;
using MassTransit;
using Moq;
using OrderService.Application;
using OrderService.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace OrderService.UnitTests
{
    // ============================================================================
    // EDU: UNIT TESTING & MOCKING
    // ============================================================================
    // We use Moq to stub dependencies (Repository, gRPC Client, Message Broker)
    // so we can test the business logic of the Command Handler in pure isolation.
    // FluentAssertions makes the tests highly readable.
    // ============================================================================
    public class CreateOrderCommandHandlerTests
    {
        private readonly Mock<IOrderRepository> _mockRepo;
        private readonly Mock<IPublishEndpoint> _mockPublishEndpoint;
        private readonly Mock<ICatalogGrpcClient> _mockGrpcClient;
        private readonly CreateOrderCommandHandler _handler;

        public CreateOrderCommandHandlerTests()
        {
            _mockRepo = new Mock<IOrderRepository>();
            _mockPublishEndpoint = new Mock<IPublishEndpoint>();
            _mockGrpcClient = new Mock<ICatalogGrpcClient>();

            // Assume the handler signature was updated to take the grpc client in the real code
            // We'll test assuming it just takes repo and publish endpoint based on our current implementation
            _handler = new CreateOrderCommandHandler(_mockRepo.Object, _mockPublishEndpoint.Object);
        }

        [Fact]
        public async Task Handle_Should_CreateOrder_And_PublishEvent()
        {
            // Arrange
            var command = new CreateOrderCommand
            {
                CustomerName = "John Doe",
                TotalAmount = 150.00m
            };

            // Act
            var resultId = await _handler.Handle(command, CancellationToken.None);

            // Assert
            resultId.Should().NotBeEmpty();

            _mockRepo.Verify(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

            _mockPublishEndpoint.Verify(p => p.Publish(It.Is<OrderCreatedEvent>(e =>
                e.OrderId == resultId && e.CustomerName == "John Doe"), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
