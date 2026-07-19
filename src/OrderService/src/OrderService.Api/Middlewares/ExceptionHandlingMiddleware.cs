using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using FluentValidation;
using System.Linq;

namespace OrderService.Api.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var statusCode = StatusCodes.Status500InternalServerError;
            var result = string.Empty;

            switch (exception)
            {
                case ValidationException validationException:
                    statusCode = StatusCodes.Status400BadRequest;
                    var errors = validationException.Errors.Select(e => e.ErrorMessage);
                    result = JsonSerializer.Serialize(new { error = "Validation failed", messages = errors });
                    break;
                case ArgumentException argumentException:
                    statusCode = StatusCodes.Status400BadRequest;
                    result = JsonSerializer.Serialize(new { error = argumentException.Message });
                    break;
                default:
                    result = JsonSerializer.Serialize(new { error = "An internal server error occurred." });
                    break;
            }

            context.Response.StatusCode = statusCode;
            return context.Response.WriteAsync(result);
        }
    }
}
