using Huellitas.Infrastructure.Persistence.Entities;

namespace Huellitas.Infrastructure.Interfaces.Repositories;

public interface IFavoritoRepository
{
    Task<Favoritos?> ObtenerPorUsuarioYMascotaAsync(int usuarioId, int mascotaId);
    Task InsertarAsync(Favoritos favorito);
    Task ActualizarAsync(Favoritos favorito);
    Task<(IEnumerable<Mascotas> Items, int Total)> ObtenerMisFavoritosAsync(int usuarioId, int pagina, int tamano);
}