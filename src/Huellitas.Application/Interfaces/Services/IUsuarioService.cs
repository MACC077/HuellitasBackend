using Huellitas.Application.DTOs.Usuarios;

namespace Huellitas.Application.Interfaces.Services
{
    public interface IUsuarioService
    {
        Task<UsuarioPerfilDTO> ObtenerPerfilAsync(int usuarioId);
        Task ActualizarPerfilAsync(int usuarioId, ActualizarPerfilDTO dto);
        Task CambiarPasswordAsync(int usuarioId, CambiarPasswordDTO dto);
        Task CambiarCorreoAsync(int usuarioId, CambiarCorreoDTO dto);
        Task DesactivarCuentaAsync(int usuarioId, DesactivarCuentaDTO dto);
        Task<PerfilAdoptanteDTO> ObtenerPerfilAdoptanteAsync(int usuarioId);
        Task ActualizarPerfilAdoptanteAsync(int usuarioId, ActualizarPerfilAdoptanteDTO dto);
    }
}
