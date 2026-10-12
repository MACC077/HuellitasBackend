using Huellitas.Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Huellitas.API.Handlers;

public class AppExceptionHandler : IExceptionHandler
{
    private readonly ILogger<AppExceptionHandler> _logger;

    public AppExceptionHandler(ILogger<AppExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is AppException appEx)
        {
            _logger.LogWarning(appEx, "Excepción controlada en {Path}", httpContext.Request.Path);

            httpContext.Response.StatusCode = appEx.StatusCode;
            httpContext.Response.ContentType = "application/json";

            var problem = new ProblemDetails
            {
                Status = appEx.StatusCode,
                Title = ((System.Net.HttpStatusCode)appEx.StatusCode).ToString(),
                Detail = appEx.Message,
                Instance = httpContext.Request.Path
            };

            await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
            return true;   // ← manejada
        }

        // No es una AppException → dejamos que otro handler o el default la maneje
        return false;
    }
}