using Huellitas.Application.Common;
using Huellitas.Application.DTOs.Trazabilidad;

namespace Huellitas.Application.Interfaces.Services;

public interface ITrazabilidadService
{
    Task<IEnumerable<LineaTiempoDTO>> ObtenerLineaTiempoAsync(int solicitudId, int usuarioId, bool esAdmin);
    Task<IEnumerable<HistorialMascotaDTO>> ObtenerHistorialMascotaAsync(int mascotaId);
    Task<PagedResult<AuditoriaDTO>> ObtenerAuditoriaAsync(int pagina, int tamano);
}
