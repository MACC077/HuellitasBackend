using Huellitas.Application.Common;
using Huellitas.Application.Common.Exceptions;
using Huellitas.Application.DTOs.Trazabilidad;
using Huellitas.Application.Interfaces.Services;
using Huellitas.Infrastructure.Enums;
using Huellitas.Infrastructure.Interfaces.Repositories;

namespace Huellitas.Application.Services;

public class TrazabilidadService : ITrazabilidadService
{
    private readonly ITrazabilidadRepository _repo;
    private readonly ISolicitudAdopcionRepository _solicitudRepo;
    private readonly IRefugioRepository _refugioRepo;
    private readonly IMascotaRepository _mascotaRepo;

    public TrazabilidadService(
        ITrazabilidadRepository repo,
        ISolicitudAdopcionRepository solicitudRepo,
        IRefugioRepository refugioRepo,
        IMascotaRepository mascotaRepo)
    {
        _repo = repo;
        _solicitudRepo = solicitudRepo;
        _refugioRepo = refugioRepo;
        _mascotaRepo = mascotaRepo;
    }

    public async Task<IEnumerable<LineaTiempoDTO>> ObtenerLineaTiempoAsync(int solicitudId, int usuarioId, bool esAdmin)
    {
        var solicitud = await _solicitudRepo.ObtenerPorIdConMascotaAsync(solicitudId)
            ?? throw AppException.NotFound("Solicitud no encontrada.");

        var esDuenio = solicitud.UsuarioId == usuarioId;
        var esRefugioDuenio = false;

        if (!esDuenio && !esAdmin && usuarioId > 0)
        {
            var refugio = await _refugioRepo.ObtenerPorUsuarioIdAsync(usuarioId);
            esRefugioDuenio = refugio != null && solicitud.Mascota.RefugioId == refugio.RefugioId;
        }

        if (!esDuenio && !esRefugioDuenio && !esAdmin)
            throw AppException.Forbidden("No tienes permiso para consultar esta solicitud.");

        var cambios = await _repo.ObtenerCambiosEstadoAsync(
            EntidadAfectada.SolicitudAdopcion.ToString(), solicitudId);

        return cambios.Select(c => new LineaTiempoDTO
        {
            EstadoAnterior = c.EstadoAnterior ?? "Sin estado previo",
            EstadoNuevo = c.EstadoNuevo,
            Observacion = c.Observacion,
            UsuarioResponsable = c.Usuario?.Nombre ?? "Sistema",
            Fecha = c.FechaCreacion
        });
    }

    public async Task<IEnumerable<HistorialMascotaDTO>> ObtenerHistorialMascotaAsync(int mascotaId)
    {
        _ = await _mascotaRepo.ObtenerPorIdAsync(mascotaId)
            ?? throw AppException.NotFound("Mascota no encontrada.");

        var bitacoras = await _repo.ObtenerBitacorasAsync(mascotaId);

        // Endpoint público: se excluyen eventos clínicos y cambios internos de estado
        // para no revelar información sensible. Solo se expone lo publicable.
        var eventosPublicos = new HashSet<byte>
        {
            (byte)TipoBitacora.Ingreso,
            (byte)TipoBitacora.Adopcion,
            (byte)TipoBitacora.Seguimiento
        };

        return bitacoras
            .Where(b => eventosPublicos.Contains(b.Tipo))
            .Select(b => new HistorialMascotaDTO
            {
                TipoEvento = ((TipoBitacora)b.Tipo).ToString(),
                // No se expone el código de constancia ni notas internas.
                Descripcion = b.Tipo == (byte)TipoBitacora.Adopcion
                    ? "Adopción finalizada."
                    : b.Descripcion,
                Fecha = b.FechaCreacion
            });
    }

    public async Task<PagedResult<AuditoriaDTO>> ObtenerAuditoriaAsync(int pagina, int tamano)
    {
        (pagina, tamano) = Paginacion.Validar(pagina, tamano);

        var (items, total) = await _repo.ObtenerAuditoriaAsync(pagina, tamano);

        return new PagedResult<AuditoriaDTO>
        {
            Items = items.Select(a => new AuditoriaDTO
            {
                AuditoriaId = a.AuditoriaId,
                Accion = a.Accion,
                EntidadAfectada = a.EntidadAfectada,
                IdEntidad = a.IdEntidad,
                ValorAnterior = a.ValorAnterior,
                ValorNuevo = a.ValorNuevo,
                UsuarioResponsable = a.Usuario?.Nombre ?? "Sistema",
                Fecha = a.FechaCreacion
            }).ToList(),
            Pagina = pagina,
            TamanoPagina = tamano,
            TotalRegistros = total
        };
    }
}
