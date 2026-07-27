using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Add Output Caching
builder.Services.AddOutputCache();

var app = builder.Build();

// Use Output Caching Middleware
app.UseOutputCache();

app.MapControllers();

app.Run();
