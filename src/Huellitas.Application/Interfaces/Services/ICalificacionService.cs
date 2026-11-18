using Huellitas.Application.Common;
using Huellitas.Application.DTOs.Refugios;

namespace Huellitas.Application.Interfaces.Services;

public interface ICalificacionService
{
    Task CalificarRefugioAsync(int usuarioId, int refugioId, CrearCalificacionDTO dto);
    Task<PagedResult<CalificacionDTO>> ObtenerCalificacionesRefugioAsync(int refugioId, int pagina, int tamano);
    Task EditarCalificacionAsync(int usuarioId, int refugioId, int calificacionId, ActualizarCalificacionDTO dto);
    Task EliminarCalificacionAsync(int usuarioId, int refugioId, int calificacionId);
}