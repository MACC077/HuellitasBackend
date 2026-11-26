using Huellitas.Infrastructure.Interfaces.Repositories;
using Huellitas.Infrastructure.Persistence.Context;
using Huellitas.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Huellitas.Infrastructure.Persistence.Repositories;

public class CalificacionRepository : ICalificacionRepository
{
    private readonly HuellitasDbContext _context;
    public CalificacionRepository(HuellitasDbContext context) => _context = context;

    public async Task<bool> ExisteCalificacionParaAdopcionAsync(int adopcionId)
    {
        return await _context.Calificaciones.AnyAsync(c => c.AdopcionId == adopcionId);
    }

    public async Task InsertarAsync(Calificaciones calificacion)
    {
        _context.Calificaciones.Add(calificacion);
        await _context.SaveChangesAsync();
    }

    public async Task<(IEnumerable<Calificaciones> Items, int Total)> ObtenerPorRefugioAsync(int refugioId, int pagina, int tamano)
    {
        var query = _context.Calificaciones
            .Include(c => c.Usuario)
            .Where(c => c.RefugioId == refugioId && c.Activo && c.EstadoModeracion == 0)
            .OrderByDescending(c => c.Fecha);

        var total = await query.CountAsync();
        var items = await query
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .ToListAsync();

        return (items, total);
    }

    public async Task<(IEnumerable<Calificaciones> Items, int Total)> ObtenerTodasAsync(
        int pagina,
        int tamano,
        byte? estadoModeracion)
    {
        var query = _context.Calificaciones
            .AsNoTracking()
            .Include(c => c.Usuario)
            .Include(c => c.Refugio)
            .Where(c => c.Activo)
            .AsQueryable();

        if (estadoModeracion.HasValue)
            query = query.Where(c => c.EstadoModeracion == estadoModeracion.Value);

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(c => c.Fecha)
            .ThenByDescending(c => c.CalificacionId)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .ToListAsync();

        return (items, total);
    }

    public async Task<Calificaciones?> ObtenerPorIdAsync(int calificacionId)
    {
        return await _context.Calificaciones
            .FirstOrDefaultAsync(c => c.CalificacionId == calificacionId && c.Activo);
    }

    public async Task ActualizarAsync(Calificaciones calificacion)
    {
        _context.Calificaciones.Update(calificacion);
        await _context.SaveChangesAsync();
    }
}