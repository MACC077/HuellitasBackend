using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Huellitas.API.Extensions;

/// <summary>
/// Configuración del límite de solicitudes para el reporte ciudadano anónimo.
/// </summary>
public sealed class ReportesRateLimitOptions
{
    public const string SectionName = "RateLimiting:Reportes";

    /// <summary>
    /// Permite desactivar el límite por entorno (por ejemplo, en desarrollo).
    /// </summary>
    public bool Habilitado { get; set; } = true;
    public int PermitLimit { get; set; } = 10;
    public int WindowSeconds { get; set; } = 60;
    public int SegmentsPerWindow { get; set; } = 6;
}

public static class RateLimitingExtensions
{
    public const string PoliticaReportes = "ReportesCiudadanos";

    public static IServiceCollection AddHuellitasRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var opciones = configuration
            .GetSection(ReportesRateLimitOptions.SectionName)
            .Get<ReportesRateLimitOptions>() ?? new ReportesRateLimitOptions();

        services.AddRateLimiter(rateLimiter =>
        {
            rateLimiter.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            rateLimiter.OnRejected = async (context, cancellationToken) =>
            {
                var httpContext = context.HttpContext;

                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    httpContext.Response.Headers.RetryAfter =
                        ((int)Math.Ceiling(retryAfter.TotalSeconds))
                        .ToString(NumberFormatInfo.InvariantInfo);
                }
                else
                {
                    // El sliding window no siempre expone RetryAfter; se usa la ventana configurada.
                    httpContext.Response.Headers.RetryAfter =
                        opciones.WindowSeconds.ToString(NumberFormatInfo.InvariantInfo);
                }

                httpContext.Response.ContentType = "application/json";
                httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Status = StatusCodes.Status429TooManyRequests,
                    Title = "TooManyRequests",
                    Detail = "Se realizaron demasiadas solicitudes. Intenta de nuevo más tarde.",
                    Instance = httpContext.Request.Path
                }, cancellationToken);
            };

            rateLimiter.AddPolicy(PoliticaReportes, httpContext =>
            {
                // En desarrollo (o cuando se desactiva por configuración) no se limita.
                if (!opciones.Habilitado)
                    return RateLimitPartition.GetNoLimiter("deshabilitado");

                var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "desconocida";

                return RateLimitPartition.GetSlidingWindowLimiter(
                    $"ip:{ip}",
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = opciones.PermitLimit,
                        Window = TimeSpan.FromSeconds(opciones.WindowSeconds),
                        SegmentsPerWindow = opciones.SegmentsPerWindow,
                        QueueLimit = 0,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        AutoReplenishment = true
                    });
            });
        });

        return services;
    }
}
