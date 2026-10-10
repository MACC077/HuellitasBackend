using Huellitas.Application.DTOs.Usuarios;

namespace Huellitas.Application.Interfaces.Services;

/// <summary>
/// Servicio de registro y verificación de cuenta (HU-USU-01).
/// Definido en la Figura 25 del documento de tesis.
/// </summary>
public interface IAutenticacionService
{
    Task<int> RegistrarAsync(CrearUsuarioDTO dto);
    Task<bool> ConfirmarCorreoAsync(string token);
    Task ReenviarCorreoVerificacionAsync(string correo);
}