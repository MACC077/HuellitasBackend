namespace Huellitas.Application.Interfaces.Services;

/// <summary>
/// Servicio de envío de correos.
/// </summary>
public interface ICorreoService
{
    Task EnviarCorreoVerificacionAsync(string destinatario, string nombre, string token);
}