// ============================================================================
// EDU: SAGA PATTERN (MassTransit State Machine)
// ============================================================================
// A Saga manages a long-running distributed transaction across multiple microservices.
// Instead of a massive distributed locking mechanism (2PC), it relies on messaging.
// - State is persisted (OrderState) via EF Core.
// - Correlates events based on OrderId.
// - Handles compensating actions (Faulted state) if subsequent steps fail.
// ============================================================================

using System;
using MassTransit;

namespace OrderService.Application
{
    // The state instance representing the Saga's state in the database
    public class OrderState : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; } = string.Empty;
        public Guid OrderId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    // The events the saga listens to
    public class InventoryReservedEvent
    {
        public Guid OrderId { get; set; }
    }

    public class InventoryFailedEvent
    {
        public Guid OrderId { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    // The State Machine (Saga)
    public class OrderStateMachine : MassTransitStateMachine<OrderState>
    {
        public State Submitted { get; private set; }
        public State Completed { get; private set; }
        public State Faulted { get; private set; }

        public Event<OrderCreatedEvent> OrderCreated { get; private set; }
        public Event<InventoryReservedEvent> InventoryReserved { get; private set; }
        public Event<InventoryFailedEvent> InventoryFailed { get; private set; }

        public OrderStateMachine()
        {
            InstanceState(x => x.CurrentState);

            Event(() => OrderCreated, x => x.CorrelateById(context => context.Message.OrderId));
            Event(() => InventoryReserved, x => x.CorrelateById(context => context.Message.OrderId));
            Event(() => InventoryFailed, x => x.CorrelateById(context => context.Message.OrderId));

            Initially(
                When(OrderCreated)
                    .Then(context =>
                    {
                        context.Saga.OrderId = context.Message.OrderId;
                        context.Saga.CreatedAt = DateTime.UtcNow;
                        // Educational comment: A Saga controls a distributed transaction.
                        // Here, after Order creation, it waits for Inventory Service response.
                    })
                    .TransitionTo(Submitted)
            );

            During(Submitted,
                When(InventoryReserved)
                    .Then(context =>
                    {
                        context.Saga.UpdatedAt = DateTime.UtcNow;
                        // Update order status logic would go here
                    })
                    .TransitionTo(Completed),
                When(InventoryFailed)
                    .Then(context =>
                    {
                        context.Saga.UpdatedAt = DateTime.UtcNow;
                        // Compensation logic (e.g. refund payment) would go here
                    })
                    .TransitionTo(Faulted)
            );
        }
    }
}
