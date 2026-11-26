using Huellitas.Infrastructure.Persistence.Entities;

namespace Huellitas.Infrastructure.Interfaces.Repositories;

public interface INecesidadRepository
{
    Task InsertarAsync(Necesidades necesidad);
    Task<Necesidades?> ObtenerPorIdAsync(int necesidadId);
    Task ActualizarAsync(Necesidades necesidad);
    Task<(IEnumerable<Necesidades> Items, int Total)> ObtenerPorRefugioAsync(int refugioId, int pagina, int tamano);
}
