using Huellitas.Application.DTOs.Usuarios;
using System;
using System.Collections.Generic;
using System.Text;

namespace Huellitas.Application.Interfaces.Services
{
    public interface IUsuarioService
    {
        Task<UsuarioPerfilDTO> ObtenerPerfilAsync(int usuarioId);
        Task ActualizarPerfilAsync(int usuarioId, ActualizarPerfilDTO dto);
        Task CambiarPasswordAsync(int usuarioId, CambiarPasswordDTO dto);
        Task CambiarCorreoAsync(int usuarioId, CambiarCorreoDTO dto);
        Task DesactivarCuentaAsync(int usuarioId, DesactivarCuentaDTO dto);
    }
}
