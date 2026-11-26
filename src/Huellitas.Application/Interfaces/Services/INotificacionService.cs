using Huellitas.Application.Common;
using Huellitas.Application.DTOs.Trazabilidad;
using Huellitas.Infrastructure.Enums;

namespace Huellitas.Application.Interfaces.Services;

public interface INotificacionService
{
    Task CrearNotificacionAsync(int usuarioId, string mensaje, CanalNotificacion canal);
    Task<PagedResult<NotificacionDTO>> ObtenerMisNotificacionesAsync(int usuarioId, int pagina, int tamano);
    Task MarcarLeidaAsync(long notificacionId, int usuarioId);
}
