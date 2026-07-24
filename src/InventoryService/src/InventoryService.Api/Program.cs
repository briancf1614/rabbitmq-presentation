using OpenTelemetry.Trace;
using MassTransit;
using InventoryService.Application;
using Microsoft.Extensions.Caching.Distributed;
using System.Text;
using System;
using StackExchange.Redis;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Add HealthChecks
builder.Services.AddHealthChecks();



// OpenTelemetry Setup

builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .AddSource("InventoryService")
            .AddAspNetCoreInstrumentation();
    });

// Redis setup
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";
    options.InstanceName = "Inventory_";
});

// RedLock setup for Distributed Locking
var redisConnectionString = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";
// Use lazy connection so it doesn't block startup if Redis is down initially in docker-compose
builder.Services.AddSingleton<RedLockNet.IDistributedLockFactory>(sp =>
{
    var multiplexer = ConnectionMultiplexer.Connect(redisConnectionString);
    var redLockMultiplexers = new List<RedLockMultiplexer> { new RedLockMultiplexer(multiplexer) };
    return RedLockFactory.Create(redLockMultiplexers);
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
app.MapHealthChecks("/health");
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
