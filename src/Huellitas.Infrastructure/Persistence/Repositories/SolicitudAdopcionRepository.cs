using Huellitas.Infrastructure.Enums;
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
                        && s.Estado != (byte)EstadoSolicitudAdopcion.Finalizada
                        && s.Estado != (byte)EstadoSolicitudAdopcion.Cancelada
                        && s.Estado != (byte)EstadoSolicitudAdopcion.Cerrada);
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

    public async Task<List<SolicitudesAdopcion>> ObtenerSolicitudesActivasPorMascotaAsync(int mascotaId)
    {
        return await _context.SolicitudesAdopcion
            .Where(s => s.MascotaId == mascotaId && s.Activo && s.Estado < (byte)EstadoSolicitudAdopcion.Finalizada)
            .ToListAsync();
    }

    public async Task EjecutarTransaccionFinalizacionAsync(
        SolicitudesAdopcion solicitudAprobada,
        List<SolicitudesAdopcion> otrasSolicitudes,
        Mascotas mascota,
        Adopciones nuevaAdopcion,
        Bitacoras bitacora)
    {
        //INICIO DE LA TRANSACCIÓN (Todo o nada)
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            _context.SolicitudesAdopcion.Update(solicitudAprobada);
            _context.SolicitudesAdopcion.UpdateRange(otrasSolicitudes);
            _context.Mascotas.Update(mascota);
            _context.Adopciones.Add(nuevaAdopcion);
            _context.Bitacoras.Add(bitacora);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw; // Lanza el error para que el Middleware lo capture
        }
    }

    public async Task InsertarVisitaAsync(VisitasEntrevista visita)
    {
        _context.VisitasEntrevista.Add(visita);
        await _context.SaveChangesAsync();
    }

    public async Task<VisitasEntrevista?> ObtenerVisitaActivaPorSolicitudAsync(int solicitudId)
    {
        return await _context.VisitasEntrevista
            .FirstOrDefaultAsync(v => v.SolicitudId == solicitudId && v.Activo);
    }

    public async Task ActualizarVisitaAsync(VisitasEntrevista visita)
    {
        _context.VisitasEntrevista.Update(visita);
        await _context.SaveChangesAsync();
    }

    public async Task<Adopciones?> ObtenerAdopcionConDetallesAsync(int solicitudId)
    {
        return await _context.Adopciones
            .Include(a => a.Solicitud)
            .ThenInclude(s => s.Usuario)
            .Include(a => a.Solicitud)
            .ThenInclude(s => s.Mascota)
            .ThenInclude(m => m.Refugio)
            .FirstOrDefaultAsync(a => a.SolicitudId == solicitudId && a.Activo);
    }

    public async Task InsertarDocumentoSoporteAsync(DocumentosSoporte documento)
    {
        _context.DocumentosSoportes.Add(documento);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<DocumentosSoporte>> ObtenerDocumentosPorSolicitudAsync(int solicitudId)
    {
        return await _context.DocumentosSoportes
            .Where(d => d.SolicitudId == solicitudId && d.Activo)
            .OrderByDescending(d => d.FechaCarga)
            .ToListAsync();
    }

    public async Task<IEnumerable<VisitasEntrevista>> ObtenerVisitasPorUsuarioAsync(int usuarioId)
    {
        return await _context.VisitasEntrevista
            .Include(v => v.Solicitud)
            .ThenInclude(s => s.Mascota)
            .ThenInclude(m => m.Refugio)
            .Include(v => v.Solicitud)
            .ThenInclude(s => s.Usuario)
            .Where(v => v.Solicitud.UsuarioId == usuarioId && v.Activo)
            .OrderByDescending(v => v.Fecha)
            .ToListAsync();
    }

    public async Task<IEnumerable<VisitasEntrevista>> ObtenerVisitasPorRefugioAsync(int refugioId)
    {
        return await _context.VisitasEntrevista
            .Include(v => v.Solicitud)
            .ThenInclude(s => s.Mascota)
            .ThenInclude(m => m.Refugio)
            .Include(v => v.Solicitud)
            .ThenInclude(s => s.Usuario)
            .Where(v => v.Solicitud.Mascota.RefugioId == refugioId && v.Activo)
            .OrderByDescending(v => v.Fecha)
            .ToListAsync();
    }
}