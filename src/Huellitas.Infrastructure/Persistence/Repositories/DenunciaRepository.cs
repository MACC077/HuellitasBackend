using Huellitas.Infrastructure.Enums;
using Huellitas.Infrastructure.Interfaces.Repositories;
using Huellitas.Infrastructure.Persistence.Context;
using Huellitas.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Huellitas.Infrastructure.Persistence.Repositories;

public class DenunciaRepository : IDenunciaRepository
{
    private readonly HuellitasDbContext _context;

    public DenunciaRepository(HuellitasDbContext context) => _context = context;

    public async Task<Denuncias> InsertarAsync(Denuncias denuncia)
    {
        _context.Denuncias.Add(denuncia);
        await _context.SaveChangesAsync();
        return denuncia;
    }

    public async Task<bool> ExisteContenidoAsync(byte tipoContenido, int idContenido)
    {
        return tipoContenido switch
        {
            (byte)TipoContenidoDenuncia.Mascota =>
                await _context.Mascotas.AnyAsync(m => m.MascotaId == idContenido && m.Activo),
            (byte)TipoContenidoDenuncia.Refugio =>
                await _context.Refugios.AnyAsync(r => r.RefugioId == idContenido && r.Activo),
            (byte)TipoContenidoDenuncia.Calificacion =>
                await _context.Calificaciones.AnyAsync(c => c.CalificacionId == idContenido && c.Activo),
            (byte)TipoContenidoDenuncia.Reporte =>
                await _context.ReportesCiudadanos.AnyAsync(r => r.ReporteId == idContenido && r.Activo),
            _ => false
        };
    }

    public async Task<bool> ExisteDenunciaPendienteAsync(int usuarioId, byte tipoContenido, int idContenido)
    {
        return await _context.Denuncias.AnyAsync(d =>
            d.Activo &&
            d.UsuarioId == usuarioId &&
            d.TipoContenido == tipoContenido &&
            d.IdContenido == idContenido &&
            d.Estado != (byte)EstadoDenuncia.Resuelta);
    }
}
