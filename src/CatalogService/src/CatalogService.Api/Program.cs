using CatalogService.Api.GraphQL;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// EDU: GRAPHQL SERVER CONFIGURATION (HotChocolate)
// ============================================================================
// Registers the GraphQL server and adds the root Query type.
// ============================================================================
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>();

var app = builder.Build();

app.MapGraphQL();
app.MapGrpcService<CatalogService.Api.Grpc.CatalogGrpcServiceImplementation>();

app.Run();
