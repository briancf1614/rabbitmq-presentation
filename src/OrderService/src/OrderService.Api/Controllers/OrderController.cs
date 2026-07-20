using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application;
using System.Threading.Tasks;

namespace OrderService.Api.Controllers
{
    [ApiController]
    [Route("api/orders")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrderController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand command)
        {
            var orderId = await _mediator.Send(command);
            return Ok(new { OrderId = orderId });
        }
    }
}
