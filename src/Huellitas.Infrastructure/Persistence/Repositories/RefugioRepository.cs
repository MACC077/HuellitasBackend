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
        // Solo mostramos refugios verificados (EstadoVerificacion = 1)
        var query = _context.Refugios
            .Where(r => r.Activo && r.EstadoVerificacion == 1)
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
}