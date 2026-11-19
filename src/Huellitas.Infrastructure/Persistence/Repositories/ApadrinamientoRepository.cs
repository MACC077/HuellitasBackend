using Huellitas.Infrastructure.Interfaces.Repositories;
using Huellitas.Infrastructure.Persistence.Context;
using Huellitas.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Huellitas.Infrastructure.Persistence.Repositories;

public class ApadrinamientoRepository : IApadrinamientoRepository
{
    private readonly HuellitasDbContext _context;
    public ApadrinamientoRepository(HuellitasDbContext context) => _context = context;

    public async Task InsertarAsync(Apadrinamientos apadrinamiento)
    {
        _context.Apadrinamientos.Add(apadrinamiento);
        await _context.SaveChangesAsync();
    }

    public async Task<Apadrinamientos?> ObtenerPorIdAsync(int apadrinamientoId)
        => await _context.Apadrinamientos
            .Include(a => a.Mascota)
            .FirstOrDefaultAsync(a => a.ApadrinamientoId == apadrinamientoId && a.Activo);

    public async Task ActualizarAsync(Apadrinamientos apadrinamiento)
    {
        _context.Apadrinamientos.Update(apadrinamiento);
        await _context.SaveChangesAsync();
    }

    public async Task<(IEnumerable<Apadrinamientos> Items, int Total)> ObtenerPorUsuarioAsync(int usuarioId, int pagina, int tamano)
    {
        var query = _context.Apadrinamientos
            .Include(a => a.Mascota)
                .ThenInclude(m => m.Refugio)
            .Where(a => a.UsuarioId == usuarioId && a.Activo)
            .OrderByDescending(a => a.FechaInicio);

        var total = await query.CountAsync();
        var items = await query
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .ToListAsync();

        return (items, total);
    }

    public async Task<(IEnumerable<Apadrinamientos> Items, int Total)> ObtenerPorRefugioAsync(int refugioId, int pagina, int tamano)
    {
        var query = _context.Apadrinamientos
            .Include(a => a.Mascota)
            .Include(a => a.Usuario)
            .Where(a => a.Mascota.RefugioId == refugioId && a.Activo)
            .OrderByDescending(a => a.FechaInicio);

        var total = await query.CountAsync();
        var items = await query
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .ToListAsync();

        return (items, total);
    }
}