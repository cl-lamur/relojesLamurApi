using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RelojesLamur.API.Common;

namespace RelojesLamur.API.Middleware;

public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOpts =
        new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Excepción no controlada: {Message}", ex.Message);
            await HandleAsync(context, ex);
        }
    }

    private static async Task HandleAsync(HttpContext context, Exception exception)
    {
        var (status, message) = exception switch
        {
            InvalidOperationException e   => (HttpStatusCode.BadRequest,          e.Message),
            UnauthorizedAccessException e => (HttpStatusCode.Unauthorized,         e.Message),
            KeyNotFoundException e        => (HttpStatusCode.NotFound,             e.Message),
            DbUpdateException e           => (HttpStatusCode.BadRequest,
                                             $"Error al guardar en base de datos: {e.InnerException?.Message ?? e.Message}"),
            _                             => (HttpStatusCode.InternalServerError,  "Error interno del servidor.")
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode  = (int)status;

        var body = JsonSerializer.Serialize(ApiResponse.Fail(message), JsonOpts);
        await context.Response.WriteAsync(body);
    }
}
