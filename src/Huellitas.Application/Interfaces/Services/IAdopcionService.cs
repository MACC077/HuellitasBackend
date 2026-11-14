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
    Task FinalizarAdopcionAsync(int solicitudId, int usuarioId);
    Task AgendarVisitaAsync(int solicitudId, AgendarVisitaDTO dto, int usuarioId);
    Task ConfirmarVisitaAsync(int solicitudId, ConfirmarVisitaDTO dto, int usuarioId);
    Task MarcarEnRevisionAsync(int solicitudId, int usuarioId);
    Task<byte[]> GenerarConstanciaPdfAsync(int solicitudId, int usuarioId);
    Task<DocumentoSoporteDTO> GuardarDocumentoAsync(int solicitudId, CrearDocumentoSoporteDTO dto, int usuarioId);
    Task<IEnumerable<DocumentoSoporteDTO>> ObtenerDocumentosAsync(int solicitudId, int usuarioId);
    Task<IEnumerable<VisitaResumenDTO>> ObtenerMisVisitasAsync(int usuarioId);
    Task<IEnumerable<VisitaResumenDTO>> ObtenerVisitasRefugioAsync(int usuarioId);
}