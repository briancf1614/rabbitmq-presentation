using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application;
using System.Threading.Tasks;
using Asp.Versioning;

namespace OrderService.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/orders")]
    public class OrderController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrderController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // ============================================================================
        // EDU: HATEOAS (Hypermedia as the Engine of Application State)
        // ============================================================================
        // Notice how the 201 Created response returns not just the ID, but actionable links
        // (like 'self' and 'cancel') so the client can dynamically discover available actions
        // without hardcoding URL structures. This is the hallmark of a Level 3 REST API (Richardson Maturity Model).
        // ============================================================================
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand command)
        {
            var orderId = await _mediator.Send(command);

            var links = new[]
            {
                new { Rel = "self", Href = Url.Link("GetOrderById", new { id = orderId, version = "1.0" }), Method = "GET" },
                new { Rel = "cancel", Href = Url.Link("CancelOrder", new { id = orderId, version = "1.0" }), Method = "DELETE" }
            };

            // Using CreatedAtRoute to follow REST semantics better than just Ok()
            return CreatedAtRoute("GetOrderById", new { id = orderId, version = "1.0" }, new { OrderId = orderId, Links = links });
        }

        [HttpGet("{id}", Name = "GetOrderById")]
        public IActionResult GetOrder(System.Guid id)
        {
            // Dummy implementation for HATEOAS demonstration
            return Ok(new { OrderId = id, Status = "Processed" });
        }

        [HttpDelete("{id}", Name = "CancelOrder")]
        public IActionResult CancelOrder(System.Guid id)
        {
            // Dummy implementation for HATEOAS demonstration
            return NoContent();
        }
    }

    [ApiController]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/orders")]
    public class OrderControllerV2 : ControllerBase
    {
        // ============================================================================
        // EDU: API VERSIONING - v2 Controller
        // ============================================================================
        // Demonstrates breaking changes without breaking older clients.
        // v2 might accept different parameters or return a different payload structure.
        // ============================================================================
        [HttpPost]
        public IActionResult CreateOrderV2()
        {
            return Ok(new { Message = "This is the v2 endpoint, expecting different payload structures." });
        }
    }
}
