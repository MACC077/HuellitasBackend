using Huellitas.Infrastructure.Interfaces.Repositories;
using Huellitas.Infrastructure.Persistence.Context;
using Huellitas.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Huellitas.Infrastructure.Persistence.Repositories;

public class RegistroClinicoRepository : IRegistroClinicoRepository
{
    private readonly HuellitasDbContext _context;
    public RegistroClinicoRepository(HuellitasDbContext context) => _context = context;

    public async Task<(IEnumerable<RegistrosClinico> Items, int Total)> ObtenerPorMascotaAsync(int mascotaId, int pagina, int tamano)
    {
        var query = _context.RegistrosClinicos
            .Where(r => r.MascotaId == mascotaId && r.Activo)
            .OrderByDescending(r => r.Fecha); // Los más recientes primero

        var total = await query.CountAsync();

        var items = await query
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .ToListAsync();

        return (items, total);
    }

    public async Task InsertarAsync(RegistrosClinico registro)
    {
        _context.RegistrosClinicos.Add(registro);
        await _context.SaveChangesAsync();
    }
}