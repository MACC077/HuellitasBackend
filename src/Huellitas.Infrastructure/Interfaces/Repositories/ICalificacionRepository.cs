using Huellitas.Infrastructure.Persistence.Entities;

namespace Huellitas.Infrastructure.Interfaces.Repositories;

public interface ICalificacionRepository
{
    Task<bool> ExisteCalificacionParaAdopcionAsync(int adopcionId);
    Task InsertarAsync(Calificaciones calificacion);
    Task<(IEnumerable<Calificaciones> Items, int Total)> ObtenerPorRefugioAsync(int refugioId, int pagina, int tamano);
    Task<Calificaciones?> ObtenerPorIdAsync(int calificacionId);
    Task ActualizarAsync(Calificaciones calificacion);
}