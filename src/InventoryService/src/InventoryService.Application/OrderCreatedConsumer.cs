using MassTransit;
using Microsoft.Extensions.Logging;
using RedLockNet;
using System;
using System.Threading.Tasks;

namespace InventoryService.Application
{
    // ============================================================================
    // EDU: DISTRIBUTED LOCKING (Redis)
    // ============================================================================
    // To prevent race conditions in a scaled-out environment (e.g., two instances
    // trying to reserve the exact same inventory item simultaneously), we use a
    // distributed lock. If instance A gets the lock, instance B must wait or fail.
    // ============================================================================
    public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
    {
        private readonly ILogger<OrderCreatedConsumer> _logger;
        private readonly IDistributedLockFactory _lockFactory;

        public OrderCreatedConsumer(ILogger<OrderCreatedConsumer> logger, IDistributedLockFactory lockFactory)
        {
            _logger = logger;
            _lockFactory = lockFactory;
        }

        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation($"Received order event for Order ID: {message.OrderId}, Customer: {message.CustomerName}");

            var resource = $"inventory-lock-{message.OrderId}"; // Assuming order id acts as the resource for demo, usually it's product id
            var expiry = TimeSpan.FromSeconds(30);
            var wait = TimeSpan.FromSeconds(10);
            var retry = TimeSpan.FromSeconds(1);

            // Attempt to acquire the distributed lock
            using (var redLock = await _lockFactory.CreateLockAsync(resource, expiry, wait, retry))
            {
                if (redLock.IsAcquired)
                {
                    _logger.LogInformation($"Lock acquired for {resource}. Reserving inventory...");
                    // Logic to update MongoDB inventory would go here
                    await Task.Delay(500); // Simulate work
                    _logger.LogInformation($"Inventory reserved for {resource}.");
                }
                else
                {
                    _logger.LogWarning($"Failed to acquire lock for {resource}. Someone else is modifying this inventory.");
                    // Throw to let MassTransit retry, or publish InventoryFailedEvent
                    throw new Exception("Could not acquire distributed lock for inventory.");
                }
            }
        }
    }
}
