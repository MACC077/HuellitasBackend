using Huellitas.Infrastructure.Interfaces.Repositories;
using Huellitas.Infrastructure.Persistence.Context;
using Huellitas.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Huellitas.Infrastructure.Persistence.Repositories;

public class TrazabilidadRepository : ITrazabilidadRepository
{
    private readonly HuellitasDbContext _context;

    public TrazabilidadRepository(HuellitasDbContext context) => _context = context;

    public async Task InsertarCambioEstadoAsync(CambiosDeEstado cambio)
    {
        _context.CambiosDeEstados.Add(cambio);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<CambiosDeEstado>> ObtenerCambiosEstadoAsync(string entidad, int idEntidad)
    {
        return await _context.CambiosDeEstados
            .Include(c => c.Usuario)
            .Where(c => c.Entidad == entidad && c.IdEntidad == idEntidad)
            .OrderByDescending(c => c.FechaCreacion)
            .ToListAsync();
    }

    public async Task InsertarBitacoraAsync(Bitacoras bitacora)
    {
        _context.Bitacoras.Add(bitacora);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Bitacoras>> ObtenerBitacorasAsync(int mascotaId)
    {
        return await _context.Bitacoras
            .Where(b => b.MascotaId == mascotaId)
            .OrderByDescending(b => b.FechaCreacion)
            .ToListAsync();
    }

    public async Task InsertarAuditoriaAsync(RegistrosAuditorias auditoria)
    {
        _context.RegistrosAuditoria.Add(auditoria);
        await _context.SaveChangesAsync();
    }

    public async Task<(IEnumerable<RegistrosAuditorias> Items, int Total)> ObtenerAuditoriaAsync(int pagina, int tamano)
    {
        var query = _context.RegistrosAuditoria
            .Include(a => a.Usuario)
            .OrderByDescending(a => a.FechaCreacion);

        var total = await query.CountAsync();
        var items = await query
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .ToListAsync();

        return (items, total);
    }
}
