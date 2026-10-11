using Huellitas.Application.DTOs.Usuarios;

namespace Huellitas.Application.Interfaces.Services;

public interface IAutenticacionService
{
    Task<int> RegistrarAsync(CrearUsuarioDTO dto);
    Task<bool> ConfirmarCorreoAsync(string token);
    Task ReenviarCorreoVerificacionAsync(string correo);
}