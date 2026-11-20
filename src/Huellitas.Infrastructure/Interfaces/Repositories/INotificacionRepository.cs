using Huellitas.Infrastructure.Persistence.Entities;

namespace Huellitas.Infrastructure.Interfaces.Repositories;

public interface INotificacionRepository
{
    Task InsertarAsync(Notificaciones notificacion);
    Task<(IEnumerable<Notificaciones> Items, int Total)> ObtenerPorUsuarioAsync(int usuarioId, int pagina, int tamano);
    Task<Notificaciones?> ObtenerPorIdAsync(long notificacionId, int usuarioId);
    Task ActualizarAsync(Notificaciones notificacion);
}
