using System.Net;

namespace Huellitas.Application.Common.Exceptions;

/// <summary>
/// Excepción única de la aplicación. Lleva el código HTTP embebido,
/// de modo que el middleware la traduce automáticamente.
/// </summary>
public class AppException : Exception
{
    public int StatusCode { get; }

    public AppException(string message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }

    // ─── Factory methods para escribir más natural ───────────────
    public static AppException BadRequest(string message) => new(message, (int)HttpStatusCode.BadRequest);
    public static AppException NotFound(string message) => new(message, (int)HttpStatusCode.NotFound);
    public static AppException Conflict(string message) => new(message, (int)HttpStatusCode.Conflict);
    public static AppException Unauthorized(string message) => new(message, (int)HttpStatusCode.Unauthorized);
    public static AppException Forbidden(string message) => new(message, (int)HttpStatusCode.Forbidden);
    public static AppException Internal(string message) => new(message, (int)HttpStatusCode.InternalServerError);

}