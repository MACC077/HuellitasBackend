using Huellitas.Application.DTOs.Trazabilidad;

namespace Huellitas.Application.Interfaces.Services;

public interface ISeguimientoService
{
    Task CrearPlanAsync(int adopcionId);
    Task<PlanSeguimientoDTO> ObtenerPlanAsync(int adopcionId, int usuarioId, bool esAdmin);
    Task EnviarReporteAsync(int seguimientoId, CrearSeguimientoDTO dto, int usuarioId);
    Task EvaluarSeguimientoAsync(int seguimientoId, byte resultado, int usuarioId);
    Task ProcesarAlertasDiariasAsync();
}
