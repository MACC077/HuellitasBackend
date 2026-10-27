using Huellitas.Application.DTOs.Adopciones;

namespace Huellitas.Application.Interfaces.Services;

public interface IAdopcionService
{
    Task<SolicitudCreadaDTO> PostularseAsync(CrearSolicitudDTO dto, int usuarioId);
}