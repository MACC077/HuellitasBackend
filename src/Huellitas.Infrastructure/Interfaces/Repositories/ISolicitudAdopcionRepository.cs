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
    Task<List<SolicitudesAdopcion>> ObtenerSolicitudesActivasPorMascotaAsync(int mascotaId);
    Task EjecutarTransaccionFinalizacionAsync(
        SolicitudesAdopcion solicitudAprobada,
        List<SolicitudesAdopcion> otrasSolicitudes,
        Mascotas mascota,
        Adopciones nuevaAdopcion,
        Bitacoras bitacora
    );
    Task InsertarVisitaAsync(VisitasEntrevista visita);
    Task<VisitasEntrevista?> ObtenerVisitaActivaPorSolicitudAsync(int solicitudId);
    Task ActualizarVisitaAsync(VisitasEntrevista visita);
    Task<Adopciones?> ObtenerAdopcionConDetallesAsync(int solicitudId);
}