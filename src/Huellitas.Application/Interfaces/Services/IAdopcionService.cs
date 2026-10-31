using Huellitas.Application.Common;
using Huellitas.Application.DTOs.Adopciones;

namespace Huellitas.Application.Interfaces.Services;

public interface IAdopcionService
{
    Task<SolicitudCreadaDTO> PostularseAsync(CrearSolicitudDTO dto, int usuarioId);
    Task<PagedResult<SolicitudResumenDTO>> ObtenerMisSolicitudesAsync(int usuarioId, int pagina, int tamano);
    Task<PagedResult<SolicitudResumenDTO>> ObtenerBandejaRefugioAsync(int usuarioId, int pagina, int tamano);
    Task AprobarSolicitudAsync(int solicitudId, AprobarSolicitudDTO dto, int usuarioId);
    Task RechazarSolicitudAsync(int solicitudId, RechazarSolicitudDTO dto, int usuarioId);
}