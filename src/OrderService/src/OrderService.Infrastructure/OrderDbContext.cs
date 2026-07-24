using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderService.Application;
using OrderService.Domain;
using System.Threading;
using System.Threading.Tasks;

namespace OrderService.Infrastructure
{
    public class OrderDbContext : DbContext
    {
        private readonly IMediator _mediator;

        // IMediator can be null during design-time migrations
        public OrderDbContext(DbContextOptions<OrderDbContext> options, IMediator mediator = null) : base(options)
        {
            _mediator = mediator;
        }

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

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Dispatch Domain Events before committing to DB
            if (_mediator != null)
            {
                await DomainEventDispatcher.DispatchEventsAsync(_mediator, this);
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
