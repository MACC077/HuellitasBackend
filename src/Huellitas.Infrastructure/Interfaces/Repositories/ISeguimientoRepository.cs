using Huellitas.Infrastructure.Persistence.Entities;

namespace Huellitas.Infrastructure.Interfaces.Repositories;

public interface ISeguimientoRepository
{
    Task<PlanesSeguimiento?> ObtenerPlanPorAdopcionAsync(int adopcionId);
    Task<PlanesSeguimiento?> ObtenerPlanPorIdAsync(int planId);
    Task InsertarPlanConSeguimientosAsync(PlanesSeguimiento plan, IEnumerable<Seguimientos> seguimientos);
    Task<Seguimientos?> ObtenerSeguimientoPorIdAsync(int seguimientoId);
    Task ActualizarSeguimientoAsync(Seguimientos seguimiento);
    Task ActualizarSeguimientoConMultimediaAsync(Seguimientos seguimiento, IEnumerable<Multimedia> multimedia);
    Task<IEnumerable<Seguimientos>> ObtenerSeguimientosPorPlanAsync(int planId);
    Task<IEnumerable<Seguimientos>> ObtenerSeguimientosVencidosAsync(DateOnly fechaLimite);
}
