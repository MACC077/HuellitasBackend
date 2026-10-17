using Huellitas.Application.DTOs.Auth;
using Huellitas.Application.DTOs.Usuarios;

namespace Huellitas.Application.Interfaces.Services;

public interface IAutenticacionService
{
    Task<int> RegistrarAsync(CrearUsuarioDTO dto);
    Task<bool> ConfirmarCorreoAsync(string token);
    Task ReenviarCorreoVerificacionAsync(string correo);
    Task<TokenResponseDTO> LoginAsync(LoginDTO dto);
    Task<TokenResponseDTO> RefreshTokenAsync(string refreshToken);
    Task LogoutAsync(string refreshToken);
    Task SolicitarRecuperacionAsync(string correo);
    Task RestablecerContrasenaAsync(string token, string nuevaContrasena);
}