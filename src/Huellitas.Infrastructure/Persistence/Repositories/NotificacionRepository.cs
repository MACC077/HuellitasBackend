using Huellitas.Infrastructure.Interfaces.Repositories;
using Huellitas.Infrastructure.Persistence.Context;
using Huellitas.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Huellitas.Infrastructure.Persistence.Repositories;

public class NotificacionRepository : INotificacionRepository
{
    private readonly HuellitasDbContext _context;

    public NotificacionRepository(HuellitasDbContext context) => _context = context;

    public async Task InsertarAsync(Notificaciones notificacion)
    {
        _context.Notificaciones.Add(notificacion);
        await _context.SaveChangesAsync();
    }

    public async Task<(IEnumerable<Notificaciones> Items, int Total)> ObtenerPorUsuarioAsync(int usuarioId, int pagina, int tamano)
    {
        var query = _context.Notificaciones
            .Where(n => n.UsuarioId == usuarioId)
            .OrderByDescending(n => n.FechaEnvio);

        var total = await query.CountAsync();
        var items = await query
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .ToListAsync();

        return (items, total);
    }

    public async Task<Notificaciones?> ObtenerPorIdAsync(long notificacionId, int usuarioId)
    {
        return await _context.Notificaciones
            .FirstOrDefaultAsync(n => n.NotificacionId == notificacionId && n.UsuarioId == usuarioId);
    }

    public async Task ActualizarAsync(Notificaciones notificacion)
    {
        _context.Notificaciones.Update(notificacion);
        await _context.SaveChangesAsync();
    }
}
