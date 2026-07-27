using MassTransit;
using InventoryService.Application;
using Microsoft.Extensions.Caching.Distributed;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Add HealthChecks
builder.Services.AddHealthChecks();



// Redis setup
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";
    options.InstanceName = "Inventory_";
});

// RabbitMQ MassTransit setup
// ============================================================================
// EDU: IDEMPOTENT CONSUMER (MongoDB Outbox/Inbox)
// ============================================================================
// In distributed systems, Message Brokers guarantee "At-Least-Once" delivery.
// This means our consumer might receive the same OrderCreatedEvent twice.
// By configuring the MassTransit MongoDb Inbox, MassTransit automatically tracks
// the MessageId. If a duplicate message arrives, it is safely ignored, preventing
// double-deduction of inventory.
// ============================================================================
builder.Services.AddMassTransit(x =>
{
    x.AddMongoDbOutbox(o =>
    {
        o.DisableInboxCleanupService();
        o.ClientFactory(provider => new MongoDB.Driver.MongoClient(builder.Configuration["MongoDb:ConnectionString"] ?? "mongodb://localhost:27017"));
        o.DatabaseFactory(provider => provider.GetRequiredService<MongoDB.Driver.IMongoClient>().GetDatabase("inventorydb"));

        o.UseBusOutbox();
    });

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
            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
            e.UseMongoDbOutbox(context);
            e.ConfigureConsumer<OrderCreatedConsumer>(context);
        });
    });
});

var app = builder.Build();

app.UseAuthorization();
app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
