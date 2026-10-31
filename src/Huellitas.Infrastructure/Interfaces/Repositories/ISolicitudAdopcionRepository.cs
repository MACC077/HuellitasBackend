using Huellitas.Infrastructure.Persistence.Entities;

namespace Huellitas.Infrastructure.Interfaces.Repositories;

public interface ISolicitudAdopcionRepository
{
    Task<bool> ExisteSolicitudActivaAsync(int usuarioId, int mascotaId);
    Task<SolicitudesAdopcion> InsertarAsync(SolicitudesAdopcion solicitud);
    Task ActualizarEstadoMascotaAsync(int mascotaId, byte nuevoEstado);
    Task<(IEnumerable<SolicitudesAdopcion> Items, int Total)> ObtenerPorRefugioAsync(int refugioId, int pagina, int tamano);
    Task<(IEnumerable<SolicitudesAdopcion> Items, int Total)> ObtenerPorAdoptanteAsync(int usuarioId, int pagina, int tamano);
    Task<SolicitudesAdopcion?> ObtenerPorIdConMascotaAsync(int solicitudId);
    Task ActualizarAsync(SolicitudesAdopcion solicitud);
}