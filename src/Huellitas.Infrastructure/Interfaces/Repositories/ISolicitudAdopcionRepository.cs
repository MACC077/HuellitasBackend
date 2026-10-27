using Huellitas.Infrastructure.Persistence.Entities;

namespace Huellitas.Infrastructure.Interfaces.Repositories;

public interface ISolicitudAdopcionRepository
{
    Task<bool> ExisteSolicitudActivaAsync(int usuarioId, int mascotaId);
    Task<SolicitudesAdopcion> InsertarAsync(SolicitudesAdopcion solicitud);
    Task ActualizarEstadoMascotaAsync(int mascotaId, byte nuevoEstado);
}