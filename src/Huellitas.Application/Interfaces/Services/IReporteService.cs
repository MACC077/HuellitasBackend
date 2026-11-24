using Huellitas.Application.Common;
using Huellitas.Application.DTOs.Reportes;

namespace Huellitas.Application.Interfaces.Services;

public interface IReporteService
{
    Task<ReporteCreadoDTO> CrearReporteAsync(CrearReporteDTO dto, int usuarioId);
    Task<PagedResult<ReporteResumenDTO>> ObtenerBandejaRefugioAsync(int usuarioId, int pagina, int tamano);
    Task<ReporteDetallePublicoDTO> ObtenerDetalleAsync(int reporteId, int usuarioId, bool esAdmin);
    Task TomarReporteAsync(int reporteId, int usuarioId, bool esAdmin);
    Task CambiarEstadoAsync(int reporteId, CambiarEstadoReporteDTO dto, int usuarioId, bool esAdmin);
    Task<int> ConvertirAFichaMascotaAsync(int reporteId, ConvertirMascotaDTO dto, int usuarioId);
    Task<IEnumerable<ReporteMapaDTO>> ObtenerReportesMapaAsync();
}
