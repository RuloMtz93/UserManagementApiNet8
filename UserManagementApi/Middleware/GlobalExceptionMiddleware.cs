using System.Net;
using System.Text.Json;
using UserManagementApi.Exceptions;

namespace UserManagementApi.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message) = exception switch
        {
            ConflictException conflictEx => (HttpStatusCode.Conflict, conflictEx.Message),          // 409
            NotFoundException notFoundEx => (HttpStatusCode.NotFound, notFoundEx.Message),          // 404
            ArgumentException argEx => (HttpStatusCode.BadRequest, argEx.Message),                  // 400
            _ => (HttpStatusCode.InternalServerError, "Ocurrió un error inesperado en el servidor.") // 500
        };

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled Exception: {Message}", exception.Message);
        }
        else
        {
            _logger.LogWarning("Handled Exception [{StatusCode}]: {Message}", statusCode, exception.Message);
        }

        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            statusCode = (int)statusCode,
            message,
            timestamp = DateTime.UtcNow
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}