// ============================================================================
// EDU: TRANSACTIONAL OUTBOX PATTERN (DbContext)
// ============================================================================
// Prevents the "Dual-Write Problem".
// Without Outbox: Save to DB -> App crashes -> Message never sent to RabbitMQ (inconsistent state).
// With Outbox: Save to DB + Save Message to Outbox table in ONE SQL Transaction.
// A background worker then polls the Outbox table and safely pushes to RabbitMQ.
// ============================================================================

using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderService.Application;
using OrderService.Domain;

namespace OrderService.Infrastructure
{
    public class OrderDbContext : DbContext
    {
        public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderState> OrderStates { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // MassTransit Outbox configuration
            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();

            // Register Saga Maps
            var orderStateMap = new OrderStateMap();
            orderStateMap.ConfigureEntity(modelBuilder.Entity<OrderState>(), modelBuilder);
        }
    }
}
