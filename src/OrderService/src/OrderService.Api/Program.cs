using FluentValidation;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderService.Api.Middlewares;
using OrderService.Application;
using OrderService.Infrastructure;
using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Serilog setup
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog();

builder.Services.AddControllers();

// Add HealthChecks\nbuilder.Services.AddHealthChecks()\n    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection"))\n    .AddRabbitMQ(rabbitConnectionString: $"amqp://guest:guest@{builder.Configuration["RabbitMQ:Host"] ?? "localhost"}:5672");\n\n// OpenTelemetry Setup\nbuilder.Services.AddOpenTelemetry()\n    .WithTracing(tracerProviderBuilder =>\n    {\n        tracerProviderBuilder\n            .AddSource("OrderService")\n            .AddAspNetCoreInstrumentation()\n            .AddHttpClientInstrumentation();\n    });

// Configure JWT Authentication
var key = Encoding.ASCII.GetBytes("SuperSecretKeyThatNeedsToBeLongEnoughForHS256");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// Database setup
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// MediatR & FluentValidation setup
builder.Services.AddValidatorsFromAssembly(typeof(CreateOrderCommandValidator).Assembly);
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(CreateOrderCommand).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
});

// RabbitMQ MassTransit setup with Transactional Outbox
builder.Services.AddMassTransit(x =>
{
    x.AddEntityFrameworkOutbox<OrderDbContext>(o =>
    {
        o.UsePostgres();
        o.UseBusOutbox();
    });

    x.AddSagaStateMachine<OrderStateMachine, OrderState>()
        .EntityFrameworkRepository(r =>
        {
            r.ConcurrencyMode = ConcurrencyMode.Pessimistic;
            r.ExistingDbContext<OrderDbContext>();
        });

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitHost = builder.Configuration["RabbitMQ:Host"] ?? "localhost";

        cfg.Host(rabbitHost, "/", h => {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ReceiveEndpoint("order-saga", e => e.ConfigureSaga<OrderState>(context));
    });
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/health");
app.MapControllers();

// Auto-migrate DB on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    db.Database.EnsureCreated();
}

app.Run();
public partial class Program { }
