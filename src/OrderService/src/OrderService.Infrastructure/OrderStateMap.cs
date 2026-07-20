using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Application;
using System.Collections.Generic;

namespace OrderService.Infrastructure
{
    public class OrderStateMap : SagaClassMap<OrderState>
    {
        protected override void Configure(EntityTypeBuilder<OrderState> entity, ModelBuilder model)
        {
            entity.Property(x => x.CurrentState).HasMaxLength(64);
            entity.Property(x => x.CreatedAt);
            entity.Property(x => x.UpdatedAt);

            // Note: CorrelationId is automatically configured as the primary key by SagaClassMap
        }

        // Expose a way to configure the entity for DbContext
        public void ConfigureEntity(EntityTypeBuilder<OrderState> entity, ModelBuilder model)
        {
             Configure(entity, model);
        }
    }
}
