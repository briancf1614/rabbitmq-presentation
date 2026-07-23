using System;
using System.Collections.Generic;
using System.Linq;

namespace CatalogService.Api.GraphQL
{
    // ============================================================================
    // EDU: GRAPHQL QUERY ROOT
    // ============================================================================
    // GraphQL allows clients to specify exactly what data they want, avoiding
    // over-fetching (getting too much data) and under-fetching (needing multiple requests).
    // This Query class is the root for all read operations in the GraphQL schema.
    // ============================================================================
    public class Query
    {
        private static readonly List<Product> _products = new()
        {
            new Product { Id = Guid.Parse("d1b2b8c9-0a6e-44d4-9d51-b01c36ea1f8f"), Name = "Enterprise License", Price = 999.99m, Description = "Full enterprise features." },
            new Product { Id = Guid.Parse("f2a4b6c8-1b7f-55e5-ac62-c12d47fa2e9g"), Name = "Standard License", Price = 499.99m, Description = "Standard features." }
        };

        public IQueryable<Product> GetProducts() => _products.AsQueryable();

        public Product GetProductById(Guid id) => _products.FirstOrDefault(p => p.Id == id);
    }
}
