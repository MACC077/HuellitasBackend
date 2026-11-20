using Huellitas.Application.Common;
using Huellitas.Application.Common.Exceptions;
using Huellitas.Application.DTOs.Trazabilidad;
using Huellitas.Application.Interfaces.Services;
using Huellitas.Infrastructure.Enums;
using Huellitas.Infrastructure.Interfaces.Repositories;
using Huellitas.Infrastructure.Persistence.Entities;

namespace Huellitas.Application.Services;

public class NotificacionService : INotificacionService
{
    private readonly INotificacionRepository _repo;

    public NotificacionService(INotificacionRepository repo) => _repo = repo;

    public async Task CrearNotificacionAsync(int usuarioId, string mensaje, CanalNotificacion canal)
    {
        await _repo.InsertarAsync(new Notificaciones
        {
            UsuarioId = usuarioId,
            Tipo = "Sistema",
            Mensaje = mensaje,
            Canal = (byte)canal,
            FechaEnvio = DateTime.Now,
            Leida = false
        });
    }

    public async Task<PagedResult<NotificacionDTO>> ObtenerMisNotificacionesAsync(int usuarioId, int pagina, int tamano)
    {
        (pagina, tamano) = Paginacion.Validar(pagina, tamano);

        var (items, total) = await _repo.ObtenerPorUsuarioAsync(usuarioId, pagina, tamano);

        return new PagedResult<NotificacionDTO>
        {
            Items = items.Select(n => new NotificacionDTO
            {
                NotificacionId = n.NotificacionId,
                Mensaje = n.Mensaje,
                Leida = n.Leida,
                FechaEnvio = n.FechaEnvio
            }).ToList(),
            Pagina = pagina,
            TamanoPagina = tamano,
            TotalRegistros = total
        };
    }

    public async Task MarcarLeidaAsync(long notificacionId, int usuarioId)
    {
        var notificacion = await _repo.ObtenerPorIdAsync(notificacionId, usuarioId)
            ?? throw AppException.NotFound("Notificación no encontrada.");

        if (notificacion.Leida)
            return;

        notificacion.Leida = true;
        await _repo.ActualizarAsync(notificacion);
    }
}
