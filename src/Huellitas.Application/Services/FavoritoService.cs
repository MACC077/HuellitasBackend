using Huellitas.Application.Common;
using Huellitas.Application.Common.Exceptions;
using Huellitas.Application.DTOs.Mascotas;
using Huellitas.Application.Interfaces.Services;
using Huellitas.Application.Mappings; 
using Huellitas.Infrastructure.Interfaces.Repositories;
using Huellitas.Infrastructure.Persistence.Entities;

namespace Huellitas.Application.Services;

public class FavoritoService : IFavoritoService
{
    private readonly IFavoritoRepository _favoritoRepo;
    private readonly IMascotaRepository _mascotaRepo;

    public FavoritoService(IFavoritoRepository favoritoRepo, IMascotaRepository mascotaRepo)
    {
        _favoritoRepo = favoritoRepo;
        _mascotaRepo = mascotaRepo;
    }

    public async Task AgregarFavoritoAsync(int usuarioId, int mascotaId)
    {
        // 1. Validar que la mascota existe
        _ = await _mascotaRepo.ObtenerPorIdAsync(mascotaId)
            ?? throw AppException.NotFound("Mascota no encontrada.");

        // 2. Verificar si ya está en favoritos
        var favoritoExistente = await _favoritoRepo.ObtenerPorUsuarioYMascotaAsync(usuarioId, mascotaId);

        if (favoritoExistente != null)
        {
            if (favoritoExistente.Activo)
                throw AppException.Conflict("Esta mascota ya está en tus favoritos.");

            // Si existía pero estaba inactivo (soft delete), lo reactivamos
            favoritoExistente.Activo = true;
            favoritoExistente.Fecha = DateTime.Now;
            await _favoritoRepo.ActualizarAsync(favoritoExistente);
            return;
        }

        // 3. Crear nuevo favorito
        var nuevoFavorito = new Favoritos
        {
            UsuarioId = usuarioId,
            MascotaId = mascotaId,
            Fecha = DateTime.Now,
            Activo = true
        };

        await _favoritoRepo.InsertarAsync(nuevoFavorito);
    }

    public async Task EliminarFavoritoAsync(int usuarioId, int mascotaId)
    {
        var favorito = await _favoritoRepo.ObtenerPorUsuarioYMascotaAsync(usuarioId, mascotaId);

        if (favorito == null || !favorito.Activo)
            throw AppException.NotFound("Esta mascota no está en tus favoritos.");

        // Soft delete
        favorito.Activo = false;
        await _favoritoRepo.ActualizarAsync(favorito);
    }

    public async Task<PagedResult<MascotaResumenDTO>> ObtenerMisFavoritosAsync(int usuarioId, int pagina, int tamano)
    {
        var (items, total) = await _favoritoRepo.ObtenerMisFavoritosAsync(usuarioId, pagina, tamano);

        return new PagedResult<MascotaResumenDTO>
        {
            Items = items.ToResumenDtoList().ToList(),
            Pagina = pagina,
            TamanoPagina = tamano,
            TotalRegistros = total
        };
    }
}