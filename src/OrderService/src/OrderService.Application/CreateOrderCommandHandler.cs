using MediatR;
using MassTransit;
using OrderService.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OrderService.Application
{
    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Guid>
    {
        private readonly IOrderRepository _repository;
        private readonly IPublishEndpoint _publishEndpoint;

        public CreateOrderCommandHandler(IOrderRepository repository, IPublishEndpoint publishEndpoint)
        {
            _repository = repository;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            var order = new Order(request.CustomerName);

            // Dummy product for demo purposes based on TotalAmount provided
            if (request.TotalAmount > 0)
            {
                 order.AddItem("Default Service Item", request.TotalAmount, 1);
            }

            await _repository.AddAsync(order, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            await _publishEndpoint.Publish(new OrderCreatedEvent
            {
                OrderId = order.Id,
                CustomerName = order.CustomerName,
                TotalAmount = order.TotalAmount
            }, cancellationToken);

            return order.Id;
        }
    }
}
