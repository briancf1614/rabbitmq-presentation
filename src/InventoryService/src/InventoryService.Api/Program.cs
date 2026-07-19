using MassTransit;
using InventoryService.Application;
using Microsoft.Extensions.Caching.Distributed;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Redis setup
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";
    options.InstanceName = "Inventory_";
});

// RabbitMQ MassTransit setup
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderCreatedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitHost = builder.Configuration["RabbitMQ:Host"] ?? "localhost";
        cfg.Host(rabbitHost, "/", h => {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ReceiveEndpoint("order-created-queue", e =>
        {
            e.ConfigureConsumer<OrderCreatedConsumer>(context);
        });
    });
});

var app = builder.Build();

app.UseAuthorization();
app.MapControllers();

// Simple endpoint to test Redis cache
app.MapGet("/api/inventory/cache-test", async (IDistributedCache cache) =>
{
    var cachedTime = await cache.GetStringAsync("lastAccessTime");
    var currentTime = DateTime.UtcNow.ToString();

    await cache.SetStringAsync("lastAccessTime", currentTime, new DistributedCacheEntryOptions {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
    });

    return Results.Ok(new { PreviousAccess = cachedTime ?? "Never", CurrentAccess = currentTime });
});

app.Run();
