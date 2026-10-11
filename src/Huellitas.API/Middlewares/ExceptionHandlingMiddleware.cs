using Huellitas.Application.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Huellitas.API.Middlewares;

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
        catch (AppException ex)
        {
            _logger.LogWarning(ex, "Excepción controlada en {Path}", context.Request.Path);
            await EscribirRespuesta(context, ex.StatusCode, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error interno no controlado en {Path}", context.Request.Path);
            await EscribirRespuesta(context, 500, "Ocurrió un error procesando tu solicitud. Intenta más tarde.");
        }
    }

    private static async Task EscribirRespuesta(HttpContext context, int statusCode, string mensaje)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = ((System.Net.HttpStatusCode)statusCode).ToString(),
            Detail = mensaje,
            Instance = context.Request.Path
        };

        await context.Response.WriteAsJsonAsync(problem);
    }
}