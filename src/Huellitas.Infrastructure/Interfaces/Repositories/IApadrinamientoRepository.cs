using Huellitas.Infrastructure.Persistence.Entities;

namespace Huellitas.Infrastructure.Interfaces.Repositories;

public interface IApadrinamientoRepository
{
    Task InsertarAsync(Apadrinamientos apadrinamiento);
    Task<Apadrinamientos?> ObtenerPorIdAsync(int apadrinamientoId);
    Task ActualizarAsync(Apadrinamientos apadrinamiento);
    Task<(IEnumerable<Apadrinamientos> Items, int Total)> ObtenerPorUsuarioAsync(int usuarioId, int pagina, int tamano);
    Task<(IEnumerable<Apadrinamientos> Items, int Total)> ObtenerPorRefugioAsync(int refugioId, int pagina, int tamano);
}