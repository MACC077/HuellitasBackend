using Huellitas.Infrastructure.Interfaces.Repositories;
using Huellitas.Infrastructure.Persistence.Context;
using Huellitas.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Huellitas.Infrastructure.Persistence.Repositories;

public class SolicitudAdopcionRepository : ISolicitudAdopcionRepository
{
    private readonly HuellitasDbContext _context;
    public SolicitudAdopcionRepository(HuellitasDbContext context) => _context = context;

    public async Task<bool> ExisteSolicitudActivaAsync(int usuarioId, int mascotaId)
    {
        return await _context.SolicitudesAdopcion
            .AnyAsync(s => s.UsuarioId == usuarioId
                        && s.MascotaId == mascotaId
                        && s.Activo
                        && s.Estado != 5
                        && s.Estado != 6
                        && s.Estado != 7);
    }
        

    public async Task<SolicitudesAdopcion> InsertarAsync(SolicitudesAdopcion solicitud)
    {
        _context.SolicitudesAdopcion.Add(solicitud);
        await _context.SaveChangesAsync();
        return solicitud;
    }

    public async Task ActualizarEstadoMascotaAsync(int mascotaId, byte nuevoEstado)
    {
        var mascota = await _context.Mascotas.FindAsync(mascotaId);
        if (mascota != null)
        {
            mascota.Estado = nuevoEstado;
            mascota.FechaUltimaActualizacion = DateTime.Now;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<(IEnumerable<SolicitudesAdopcion> Items, int Total)> ObtenerPorRefugioAsync(int refugioId, int pagina, int tamano)
    {
        var query = _context.SolicitudesAdopcion
            .Include(s => s.Usuario)
            .Include(s => s.Mascota)
            .Where(s => s.Mascota.RefugioId == refugioId && s.Activo)
            .OrderByDescending(s => s.FechaEnvio);

        var total = await query.CountAsync();
        var items = await query
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .ToListAsync();

        return (items, total);
    }

    public async Task<(IEnumerable<SolicitudesAdopcion> Items, int Total)> ObtenerPorAdoptanteAsync(int usuarioId, int pagina, int tamano)
    {
        var query = _context.SolicitudesAdopcion
            .Include(s => s.Mascota)
                .ThenInclude(m => m.Multimedia)
            .Where(s => s.UsuarioId == usuarioId && s.Activo)
            .OrderByDescending(s => s.FechaEnvio);

        var total = await query.CountAsync();
        var items = await query
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .ToListAsync();

        return (items, total);
    }

    public async Task<SolicitudesAdopcion?> ObtenerPorIdConMascotaAsync(int solicitudId)
    {
        return await _context.SolicitudesAdopcion
            .Include(s => s.Mascota)
            .FirstOrDefaultAsync(s => s.SolicitudId == solicitudId && s.Activo);
    }

    public async Task ActualizarAsync(SolicitudesAdopcion solicitud)
    {
        _context.SolicitudesAdopcion.Update(solicitud);
        await _context.SaveChangesAsync();
    }
}