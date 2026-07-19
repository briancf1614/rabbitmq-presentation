using MassTransit;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace InventoryService.Application
{
    public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
    {
        private readonly ILogger<OrderCreatedConsumer> _logger;

        public OrderCreatedConsumer(ILogger<OrderCreatedConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation($"Received order event for Order ID: {message.OrderId}, Customer: {message.CustomerName}");
            // In a real app, logic to update inventory would go here.
            await Task.CompletedTask;
        }
    }
}
