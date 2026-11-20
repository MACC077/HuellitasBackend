using Huellitas.Application.Common.Exceptions;
using Huellitas.Application.DTOs.Trazabilidad;
using Huellitas.Application.Interfaces.Services;
using Huellitas.Infrastructure.Enums;
using Huellitas.Infrastructure.Interfaces.Repositories;
using Huellitas.Infrastructure.Persistence.Entities;

namespace Huellitas.Application.Services;

public class SeguimientoService : ISeguimientoService
{
    private static readonly int[] MesesSeguimiento = { 1, 3, 6 };

    private readonly ISeguimientoRepository _repo;
    private readonly ISolicitudAdopcionRepository _solicitudRepo;
    private readonly IRefugioRepository _refugioRepo;
    private readonly INotificacionService _notificacionService;

    public SeguimientoService(
        ISeguimientoRepository repo,
        ISolicitudAdopcionRepository solicitudRepo,
        IRefugioRepository refugioRepo,
        INotificacionService notificacionService)
    {
        _repo = repo;
        _solicitudRepo = solicitudRepo;
        _refugioRepo = refugioRepo;
        _notificacionService = notificacionService;
    }

    public async Task CrearPlanAsync(int adopcionId)
    {
        _ = await _solicitudRepo.ObtenerAdopcionPorIdConDetallesAsync(adopcionId)
            ?? throw AppException.NotFound("Adopción no encontrada.");

        // Idempotente: si ya existe un plan activo no se crea otro.
        if (await _repo.ObtenerPlanPorAdopcionAsync(adopcionId) != null)
            return;

        var hoy = DateOnly.FromDateTime(DateTime.Now);

        var plan = new PlanesSeguimiento
        {
            AdopcionId = adopcionId,
            CantidadSeguimientos = (byte)MesesSeguimiento.Length,
            Periodicidad = "1, 3, 6 meses",
            Activo = true,
            FechaCreacion = DateTime.Now
        };

        var seguimientos = new List<Seguimientos>();
        for (var i = 0; i < MesesSeguimiento.Length; i++)
        {
            seguimientos.Add(new Seguimientos
            {
                Numero = (byte)(i + 1),
                FechaProgramada = hoy.AddMonths(MesesSeguimiento[i]),
                Estado = (byte)EstadoSeguimiento.Pendiente,
                Activo = true,
                FechaCreacion = DateTime.Now
            });
        }

        await _repo.InsertarPlanConSeguimientosAsync(plan, seguimientos);
    }

    public async Task<PlanSeguimientoDTO> ObtenerPlanAsync(int adopcionId, int usuarioId, bool esAdmin)
    {
        var adopcion = await _solicitudRepo.ObtenerAdopcionPorIdConDetallesAsync(adopcionId)
            ?? throw AppException.NotFound("Adopción no encontrada.");

        await ValidarAccesoAdopcionAsync(adopcion, usuarioId, esAdmin);

        var plan = await _repo.ObtenerPlanPorAdopcionAsync(adopcionId)
            ?? throw AppException.NotFound("Plan de seguimiento no encontrado.");

        var seguimientos = await _repo.ObtenerSeguimientosPorPlanAsync(plan.PlanId);

        return new PlanSeguimientoDTO
        {
            PlanId = plan.PlanId,
            AdopcionId = plan.AdopcionId,
            Seguimientos = seguimientos.Select(s => new SeguimientoDTO
            {
                SeguimientoId = s.SeguimientoId,
                Numero = s.Numero,
                FechaProgramada = s.FechaProgramada,
                FechaEnvio = s.FechaEnvio,
                Estado = ((EstadoSeguimiento)s.Estado).ToString(),
                Resultado = s.Resultado?.ToString(),
                FotosUrls = s.Multimedia.Where(m => m.Activo).Select(m => m.Archivo).ToList()
            }).ToList()
        };
    }

    public async Task EnviarReporteAsync(int seguimientoId, CrearSeguimientoDTO dto, int usuarioId)
    {
        var seguimiento = await _repo.ObtenerSeguimientoPorIdAsync(seguimientoId)
            ?? throw AppException.NotFound("Seguimiento no encontrado.");

        // Autorización por recurso: el adoptante debe ser el dueño de la adopción.
        if (seguimiento.Plan.Adopcion.Solicitud.UsuarioId != usuarioId)
            throw AppException.Forbidden("No tienes permiso para reportar este seguimiento.");

        if (seguimiento.Estado == (byte)EstadoSeguimiento.Enviado)
            throw AppException.Conflict("Este seguimiento ya fue enviado.");

        seguimiento.FechaEnvio = DateTime.Now;
        seguimiento.EstadoSalud = dto.EstadoSalud;
        seguimiento.Adaptacion = dto.Adaptacion;
        seguimiento.Comentarios = dto.Comentarios;
        seguimiento.Estado = (byte)EstadoSeguimiento.Enviado;
        seguimiento.FechaModificacion = DateTime.Now;

        var fotos = dto.FotosUrls ?? new List<string>();
        var multimedia = fotos
            .Where(url => !string.IsNullOrWhiteSpace(url))
            .Select((url, index) => new Multimedia
            {
                SeguimientoId = seguimiento.SeguimientoId,
                Tipo = 0,
                Archivo = url,
                EsPrincipal = index == 0,
                FechaCarga = DateTime.Now,
                Activo = true
            })
            .ToList();

        await _repo.ActualizarSeguimientoConMultimediaAsync(seguimiento, multimedia);
    }

    public async Task EvaluarSeguimientoAsync(int seguimientoId, byte resultado, int usuarioId)
    {
        var seguimiento = await _repo.ObtenerSeguimientoPorIdAsync(seguimientoId)
            ?? throw AppException.NotFound("Seguimiento no encontrado.");

        var refugio = await _refugioRepo.ObtenerPorUsuarioIdAsync(usuarioId)
            ?? throw AppException.BadRequest("Usuario no asociado a un refugio.");

        // Autorización por recurso: el seguimiento debe pertenecer a una mascota del refugio.
        if (seguimiento.Plan.Adopcion.Solicitud.Mascota.RefugioId != refugio.RefugioId)
            throw AppException.Forbidden("No tienes permiso para evaluar este seguimiento.");

        if (seguimiento.Estado != (byte)EstadoSeguimiento.Enviado)
            throw AppException.Conflict("Solo se pueden evaluar seguimientos enviados por el adoptante.");

        seguimiento.Resultado = resultado;
        seguimiento.FechaModificacion = DateTime.Now;

        await _repo.ActualizarSeguimientoAsync(seguimiento);

        await _notificacionService.CrearNotificacionAsync(
            seguimiento.Plan.Adopcion.Solicitud.UsuarioId,
            $"El refugio evaluó el seguimiento #{seguimiento.Numero} de tu adopción.",
            CanalNotificacion.InApp);
    }

    public async Task ProcesarAlertasDiariasAsync()
    {
        var vencidos = await _repo.ObtenerSeguimientosVencidosAsync(DateOnly.FromDateTime(DateTime.Now));

        foreach (var seguimiento in vencidos)
        {
            seguimiento.Estado = (byte)EstadoSeguimiento.Vencido;
            seguimiento.FechaModificacion = DateTime.Now;

            await _repo.ActualizarSeguimientoAsync(seguimiento);

            await _notificacionService.CrearNotificacionAsync(
                seguimiento.Plan.Adopcion.Solicitud.UsuarioId,
                $"El seguimiento #{seguimiento.Numero} de tu adopción está vencido.",
                CanalNotificacion.InApp);
        }
    }

    private async Task ValidarAccesoAdopcionAsync(Adopciones adopcion, int usuarioId, bool esAdmin)
    {
        if (esAdmin || adopcion.Solicitud.UsuarioId == usuarioId)
            return;

        if (usuarioId > 0)
        {
            var refugio = await _refugioRepo.ObtenerPorUsuarioIdAsync(usuarioId);
            if (refugio != null && adopcion.Solicitud.Mascota.RefugioId == refugio.RefugioId)
                return;
        }

        throw AppException.Forbidden("No tienes permiso para consultar este plan de seguimiento.");
    }
}
