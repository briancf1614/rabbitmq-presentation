using MediatR;
using System;

namespace OrderService.Application
{
    public class GetOrderQuery : IRequest<OrderDto>
    {
        public Guid OrderId { get; set; }
    }

    public class OrderDto
    {
        public Guid Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
