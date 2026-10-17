namespace Huellitas.Infrastructure.Interfaces.Services;

/// <summary>
/// Servicio de envío de correos.
/// </summary>
public interface ICorreoService
{
    Task EnviarCorreoVerificacionAsync(string destinatario, string nombre, string token);
    Task EnviarCorreoRecuperacionAsync(string destinatario, string nombre, string token);
}