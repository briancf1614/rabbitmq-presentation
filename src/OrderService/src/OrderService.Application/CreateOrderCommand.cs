using MediatR;
using System;

namespace OrderService.Application
{
    public class CreateOrderCommand : IRequest<Guid>
    {
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
    }
}
