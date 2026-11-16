using Huellitas.Application.Common;
using Huellitas.Application.DTOs.Mascotas;

namespace Huellitas.Application.Interfaces.Services;

public interface IFavoritoService
{
    Task AgregarFavoritoAsync(int usuarioId, int mascotaId);
    Task EliminarFavoritoAsync(int usuarioId, int mascotaId);
    Task<PagedResult<MascotaResumenDTO>> ObtenerMisFavoritosAsync(int usuarioId, int pagina, int tamano);
}