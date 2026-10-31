using Huellitas.Infrastructure.Enums;
using Huellitas.Infrastructure.Interfaces.Repositories;
using Huellitas.Infrastructure.Persistence.Context;
using Huellitas.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Huellitas.Infrastructure.Persistence.Repositories;

public class MascotaRepository : IMascotaRepository
{
    private readonly HuellitasDbContext _context;
    public MascotaRepository(HuellitasDbContext context) => _context = context;

    public async Task<Mascotas> InsertarAsync(Mascotas mascota)
    {
        _context.Mascotas.Add(mascota);
        await _context.SaveChangesAsync();
        return mascota;
    }

    public async Task ActualizarAsync(Mascotas mascota)
    {
        _context.Mascotas.Update(mascota);
        await _context.SaveChangesAsync();
    }

    public async Task<Mascotas?> ObtenerPorIdAsync(int mascotaId)
    {
        return await _context.Mascotas
           .Include(m => m.Multimedia)
           .FirstOrDefaultAsync(m => m.MascotaId == mascotaId);
    }
     
    public async Task<bool> TieneSolicitudesActivasAsync(int mascotaId)
    {
        return await _context.SolicitudesAdopcion
            .AnyAsync(s => s.MascotaId == mascotaId 
                        && s.Activo 
                        && s.Estado != 5 
                        && s.Estado != 6 
                        && s.Estado != 7);
    }
        

    public async Task<(IEnumerable<Mascotas> Items, int Total)> ObtenerMisMascotasAsync(int refugioId, int pagina, int tamano)
    {
        var query = _context.Mascotas
            .Include(m => m.Especie)
            .Include(m => m.Raza)
            .Include(m => m.Multimedia)
            .Where(m => m.RefugioId == refugioId && m.Activo)
            .OrderByDescending(m => m.FechaCreacion);

        var total = await query.CountAsync();
        var items = await query.Skip((pagina - 1) * tamano).Take(tamano).ToListAsync();
        return (items, total);
    }

    public async Task InsertarBitacoraAsync(Bitacoras bitacora)
    {
        _context.Bitacoras.Add(bitacora);
        await _context.SaveChangesAsync();
    }

    public async Task InsertarMultimediaAsync(Multimedia multimedia)
    {
        _context.Multimedia.Add(multimedia);
        await _context.SaveChangesAsync();
    }

    public async Task EliminarMultimediaAsync(int mascotaId)
    {
        var fotos = await _context.Multimedia.Where(m => m.MascotaId == mascotaId).ToListAsync();
        _context.Multimedia.RemoveRange(fotos);
        await _context.SaveChangesAsync();
    }

    public async Task<(IEnumerable<Mascotas> Items, int Total)> ObtenerCatalogoPublicoAsync(int pagina, int tamano, int? especieId, int? tamanoId, byte? sexo)
    {
        var query = _context.Mascotas
       .Include(m => m.Especie)
       .Include(m => m.Raza)
       .Include(m => m.Multimedia)
       .Where(m => m.Activo &&
                  (m.Estado == (byte)EstadoMascota.Disponible ||
                   m.Estado == (byte)EstadoMascota.EnProceso))
       .AsQueryable();

        // Aplicar filtros
        if (especieId.HasValue)
            query = query.Where(m => m.EspecieId == especieId.Value);

        if (tamanoId.HasValue)
            query = query.Where(m => m.TamanoId == tamanoId.Value);

        if (sexo.HasValue)
            query = query.Where(m => m.Sexo == sexo.Value);

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(m => m.FechaCreacion)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .ToListAsync();

        return (items, total);
    }

    public async Task<Mascotas?> ObtenerDetallePublicoAsync(int mascotaId)
    {
        return await _context.Mascotas
         .Include(m => m.Especie)
         .Include(m => m.Raza)
         .Include(m => m.Refugio)
         .Include(m => m.Multimedia)
         .FirstOrDefaultAsync(m => m.MascotaId == mascotaId &&
                                   m.Activo &&
                                   (m.Estado == (byte)EstadoMascota.Disponible ||
                                    m.Estado == (byte)EstadoMascota.EnProceso));
    }
}