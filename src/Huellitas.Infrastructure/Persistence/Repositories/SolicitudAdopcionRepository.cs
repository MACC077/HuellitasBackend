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
}