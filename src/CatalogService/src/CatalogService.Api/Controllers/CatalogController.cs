using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CatalogService.Api.Controllers
{
    [ApiController]
    [Route("api/catalog")]
    public class CatalogController : ControllerBase
    {
        private static readonly List<Product> _products = new()
        {
            new Product { Id = Guid.Parse("d1b2b8c9-0a6e-44d4-9d51-b01c36ea1f8f"), Name = "Enterprise License", Price = 999.99m, Description = "Full enterprise features." },
            new Product { Id = Guid.Parse("f2a4b6c8-1b7f-55e5-ac62-c12d47fa2e9g"), Name = "Standard License", Price = 499.99m, Description = "Standard features." }
        };

        // ============================================================================
        // EDU: OUTPUT CACHING
        // ============================================================================
        // Output Caching caches the full HTTP response (headers and body).
        // Since product catalog data rarely changes, this dramatically increases throughput
        // and reduces load on the underlying database. The cache is kept for 10 minutes.
        // ============================================================================
        [HttpGet]
        [Microsoft.AspNetCore.OutputCaching.OutputCache(Duration = 600)]
        public IActionResult GetProducts()
        {
            return Ok(_products);
        }

        [HttpGet("{id}")]
        [Microsoft.AspNetCore.OutputCaching.OutputCache(Duration = 600)]
        public IActionResult GetProductById(Guid id)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product == null) return NotFound();
            return Ok(product);
        }
    }

    public class Product
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
