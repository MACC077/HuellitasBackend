using Huellitas.Infrastructure.Interfaces.Repositories;
using Huellitas.Infrastructure.Persistence.Context;
using Huellitas.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Huellitas.Infrastructure.Persistence.Repositories;

public class NecesidadRepository : INecesidadRepository
{
    private readonly HuellitasDbContext _context;
    public NecesidadRepository(HuellitasDbContext context) => _context = context;

    public async Task InsertarAsync(Necesidades necesidad)
    {
        _context.Necesidades.Add(necesidad);
        await _context.SaveChangesAsync();
    }

    public async Task<Necesidades?> ObtenerPorIdAsync(int necesidadId)
        => await _context.Necesidades.FirstOrDefaultAsync(n => n.NecesidadId == necesidadId && n.Activo);

    public async Task ActualizarAsync(Necesidades necesidad)
    {
        _context.Necesidades.Update(necesidad);
        await _context.SaveChangesAsync();
    }

    public async Task<(IEnumerable<Necesidades> Items, int Total)> ObtenerPorRefugioAsync(int refugioId, int pagina, int tamano)
    {
        var query = _context.Necesidades
            .Where(n => n.RefugioId == refugioId && n.Activo)
            .OrderByDescending(n => n.FechaPublicacion);

        var total = await query.CountAsync();
        var items = await query
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .ToListAsync();

        return (items, total);
    }
}