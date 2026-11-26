using Huellitas.Infrastructure.Enums;
using Huellitas.Infrastructure.Interfaces.Repositories;
using Huellitas.Infrastructure.Persistence.Context;
using Huellitas.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Huellitas.Infrastructure.Persistence.Repositories;

public class RefugioRepository : IRefugioRepository
{
    private readonly HuellitasDbContext _context;
    public RefugioRepository(HuellitasDbContext context) => _context = context;

    public async Task<Refugios?> ObtenerPorUsuarioIdAsync(int usuarioId)
    {
        // El scaffold de EF Core genera la propiedad de navegación 'Refugio' en Usuarios
        // gracias a la FK RefugioId.
        var usuario = await _context.Usuarios
            .Include(u => u.Refugio)
            .FirstOrDefaultAsync(u => u.UsuarioId == usuarioId);

        return usuario?.Refugio;
    }

    public async Task<Refugios?> ObtenerPorIdAsync(int refugioId)
    {
        return await _context.Refugios
            .Include(r => r.RequisitosAdopcions.Where(req => req.Activo))
            .Include(r => r.Soportes.Where(s => s.Activo))
            .FirstOrDefaultAsync(r => r.RefugioId == refugioId);
    }

    public async Task<Refugios> InsertarAsync(Refugios refugio)
    {
        _context.Refugios.Add(refugio);
        await _context.SaveChangesAsync();
        return refugio;
    }

    public async Task ActualizarAsync(Refugios refugio)
    {
        _context.Refugios.Update(refugio);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<RequisitosAdopcion>> ObtenerRequisitosPorRefugioAsync(int refugioId) 
    {
        return await _context.RequisitosAdopcions
           .Where(r => r.RefugioId == refugioId && r.Activo)
           .OrderBy(r => r.Orden)
           .ToListAsync();
    }
      
    public async Task InsertarRequisitoAsync(RequisitosAdopcion requisito)
    {
        _context.RequisitosAdopcions.Add(requisito);
        await _context.SaveChangesAsync();
    }

    public async Task EliminarRequisitoAsync(int requisitoId)
    {
        var req = await _context.RequisitosAdopcions.FindAsync(requisitoId);
        if (req != null)
        {
            req.Activo = false; // Borrado lógico
            await _context.SaveChangesAsync();
        }
    }

    public async Task<(IEnumerable<Refugios> Items, int Total)> ObtenerRefugiosPublicosAsync(int pagina, int tamano)
    {
        // Solo mostramos refugios verificados
        var query = _context.Refugios
            .Where(r => r.Activo && r.EstadoVerificacion == (byte)EstadoVerificacionRefugio.Verificado)
            .OrderBy(r => r.Nombre);

        var total = await query.CountAsync();
        var items = await query
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .ToListAsync();

        return (items, total);
    }

    public async Task<Refugios?> ObtenerPerfilPublicoPorIdAsync(int refugioId)
    {
        return await _context.Refugios
            .Include(r => r.RequisitosAdopcions.Where(req => req.Activo))
            .Include(r => r.Mascota.Where(m => m.Activo && (m.Estado == 0 || m.Estado == 1))) // Sus mascotas disponibles
            .FirstOrDefaultAsync(r => r.RefugioId == refugioId && r.Activo);
    }

    public async Task<bool> ExisteNitAsync(string nit)
    {
        return await _context.Refugios.AnyAsync(r => r.Nit == nit);
    }

    public async Task<(IEnumerable<Refugios> Items, int Total)> ObtenerRefugiosPorEstadoAsync(
        byte estado,
        int pagina,
        int tamano)
    {
        var query = _context.Refugios
            .AsNoTracking()
            .Include(r => r.Soportes.Where(s => s.Activo))
            .Where(r => r.Activo && r.EstadoVerificacion == estado)
            .OrderBy(r => r.FechaCreacion)
            .ThenBy(r => r.RefugioId);

        var total = await query.CountAsync();

        var items = await query
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .ToListAsync();

        return (items, total);
    }

    public async Task InsertarSoportesAsync(IEnumerable<RefugioSoportes> soportes)
    {
        _context.RefugioSoportes.AddRange(soportes);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<RefugioSoportes>> ObtenerSoportesAsync(int refugioId)
    {
        return await _context.RefugioSoportes
            .AsNoTracking()
            .Where(s => s.RefugioId == refugioId && s.Activo)
            .OrderBy(s => s.SoporteId)
            .ToListAsync();
    }
}