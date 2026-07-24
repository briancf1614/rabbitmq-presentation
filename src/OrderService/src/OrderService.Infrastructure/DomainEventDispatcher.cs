using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderService.Domain;
using System.Linq;
using System.Threading.Tasks;

namespace OrderService.Infrastructure
{
    public static class DomainEventDispatcher
    {
        public static async Task DispatchEventsAsync(IMediator mediator, DbContext ctx)
        {
            var domainEntities = ctx.ChangeTracker
                .Entries<Entity>()
                .Where(x => x.Entity.DomainEvents != null && x.Entity.DomainEvents.Any());

            var domainEvents = domainEntities
                .SelectMany(x => x.Entity.DomainEvents)
                .ToList();

            domainEntities.ToList()
                .ForEach(entity => entity.Entity.ClearDomainEvents());

            foreach (var domainEvent in domainEvents)
            {
                // We wrap domain events in MediatR notifications
                // The INotification interface must be implemented by the wrapper or the event itself.
                // For simplicity, assuming handlers can accept object, but usually you'd cast.
                // Here we just publish them via MediatR to trigger any local side-effects.
                await mediator.Publish(domainEvent);
            }
        }
    }
}
