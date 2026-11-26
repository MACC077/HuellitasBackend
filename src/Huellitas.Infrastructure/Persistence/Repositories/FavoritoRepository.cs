using Huellitas.Infrastructure.Interfaces.Repositories;
using Huellitas.Infrastructure.Persistence.Context;
using Huellitas.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Huellitas.Infrastructure.Persistence.Repositories;

public class FavoritoRepository : IFavoritoRepository
{
    private readonly HuellitasDbContext _context;
    public FavoritoRepository(HuellitasDbContext context) => _context = context;

    public async Task<Favoritos?> ObtenerPorUsuarioYMascotaAsync(int usuarioId, int mascotaId)
    {
        return await _context.Favoritos
            .FirstOrDefaultAsync(f => f.UsuarioId == usuarioId && f.MascotaId == mascotaId);
    }

    public async Task InsertarAsync(Favoritos favorito)
    {
        _context.Favoritos.Add(favorito);
        await _context.SaveChangesAsync();
    }

    public async Task ActualizarAsync(Favoritos favorito)
    {
        _context.Favoritos.Update(favorito);
        await _context.SaveChangesAsync();
    }

    public async Task<(IEnumerable<Mascotas> Items, int Total)> ObtenerMisFavoritosAsync(int usuarioId, int pagina, int tamano)
    {
       
        var query = _context.Mascotas
            .Where(m => m.Activo) // No mostrar mascotas dadas de baja
            .Where(m => _context.Favoritos.Any(f =>
                f.MascotaId == m.MascotaId &&
                f.UsuarioId == usuarioId &&
                f.Activo)) // Solo las que están en favoritos
            .Include(m => m.Especie)
            .Include(m => m.Raza)
            .Include(m => m.Multimedia)
            .AsQueryable();

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(m => m.FechaCreacion)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .ToListAsync();

        return (items, total);
    }
}