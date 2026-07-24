using System;

namespace OrderService.Domain
{
    // ============================================================================
    // EDU: DOMAIN EVENTS
    // ============================================================================
    // Represents something that happened internally in the domain.
    // Unlike Integration Events (which go to RabbitMQ for other services),
    // Domain Events stay within the same microservice to trigger side effects
    // in the SAME database transaction.
    // ============================================================================
    public class OrderStartedDomainEvent : MediatR.INotification
    {
        public Guid OrderId { get; }
        public OrderStartedDomainEvent(Guid orderId) => OrderId = orderId;
    }
}
