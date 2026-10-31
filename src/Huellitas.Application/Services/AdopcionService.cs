using Huellitas.Application.Common;
using Huellitas.Application.Common.Exceptions;
using Huellitas.Application.DTOs.Adopciones;
using Huellitas.Application.Interfaces.Services;
using Huellitas.Application.Mappings;
using Huellitas.Infrastructure.Enums;
using Huellitas.Infrastructure.Interfaces.Repositories;
using Huellitas.Infrastructure.Persistence.Entities;
using System.Text.Json;

namespace Huellitas.Application.Services;

public class AdopcionService : IAdopcionService
{
    private readonly ISolicitudAdopcionRepository _solicitudRepo;
    private readonly IMascotaRepository _mascotaRepo;
    private readonly IRefugioRepository _refugioRepo;
    private readonly IPerfilAdoptanteRepository _perfilRepo;

    public AdopcionService(ISolicitudAdopcionRepository solicitudRepo, IMascotaRepository mascotaRepo, IRefugioRepository refugioRepo, IPerfilAdoptanteRepository perfilRepo)
    {
        _solicitudRepo = solicitudRepo;
        _mascotaRepo = mascotaRepo;
        _refugioRepo = refugioRepo;
        _perfilRepo = perfilRepo;
    }


    public async Task<SolicitudCreadaDTO> PostularseAsync(CrearSolicitudDTO dto, int usuarioId)
    {

        //Validacion del perfil adoptange: Debe existir y estar completo al 100%
        var perfil = await _perfilRepo.ObtenerPorUsuarioIdAsync(usuarioId);

        if (perfil == null)
            throw AppException.BadRequest("Debes crear tu perfil de adoptante antes de postularte.");

        bool perfilCompleto = perfil.TipoVivienda.HasValue &&
                              !string.IsNullOrWhiteSpace(perfil.EspacioDisponible) &&
                              perfil.PresenciaDeNinos.HasValue &&
                              perfil.OtrasMascotas.HasValue &&
                              !string.IsNullOrWhiteSpace(perfil.TiempoDisponible) &&
                              perfil.ExperienciaPrevia.HasValue;

        if (!perfilCompleto)
            throw AppException.BadRequest("Debes completar el 100% de tu perfil de adoptante para enviar una solicitud.");

        //Validar que la mascota exista y esté Disponible (0) o En Proceso (1)
        var mascota = await _mascotaRepo.ObtenerPorIdAsync(dto.MascotaId)
            ?? throw AppException.NotFound("Mascota no encontrada.");

        if (mascota.Estado == (byte)EstadoMascota.Adoptada || mascota.Estado == (byte)EstadoMascota.NoDisponible)
            throw AppException.Conflict("Esta mascota ya no está disponible para adopción.");

        //Validar que ESTE usuario no tenga una solicitud activa para ESTA mascota
        if (await _solicitudRepo.ExisteSolicitudActivaAsync(usuarioId, dto.MascotaId))
            throw AppException.Conflict("Ya tienes una solicitud activa para esta mascota.");

        //Mapeo DTO -> Entidad
        var respuestasSerializadas = SerializarRespuestas(dto.RespuestasFormulario);
        var solicitud = dto.ToEntity(respuestasSerializadas);

        //Reglas de negocio (campos generados)
        solicitud.UsuarioId = usuarioId;
        solicitud.Codigo = $"SOL-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}";
        solicitud.FechaEnvio = DateTime.Now;
        solicitud.Estado = 0;
        solicitud.Activo = true;
        solicitud.FechaCreacion = DateTime.Now;

        await _solicitudRepo.InsertarAsync(solicitud);

        //Si la mascota estaba Disponible (0), la pasamos a En Proceso (1)
        if (mascota.Estado == (byte)EstadoMascota.Disponible)
        {
            await _solicitudRepo.ActualizarEstadoMascotaAsync(dto.MascotaId, (byte)EstadoMascota.EnProceso);
        }

        // Retornar el DTO descriptivo
        return solicitud.ToSolicitudCreadaDto(mascota.Nombre);
    }

    // ═══════════════════════════════════════════════════════
    // Helper para serialización flexible
    // ═══════════════════════════════════════════════════════
    private static string SerializarRespuestas(object respuestas)
    {
        if (respuestas == null)
            return string.Empty;

        // Si el frontend envía un string simple, lo guardamos tal cual
        if (respuestas is string textoSimple)
            return textoSimple;

        // Si envía un objeto complejo, lo serializamos a JSON
        return JsonSerializer.Serialize(respuestas);
    }

    public async Task<PagedResult<SolicitudResumenDTO>> ObtenerMisSolicitudesAsync(int usuarioId, int pagina, int tamano)
    {
        var (items, total) = await _solicitudRepo.ObtenerPorAdoptanteAsync(usuarioId, pagina, tamano);

        return new PagedResult<SolicitudResumenDTO>
        {
            Items = items.Select(s => s.ToResumenDto()).ToList(),
            Pagina = pagina,
            TamanoPagina = tamano,
            TotalRegistros = total
        };
    }

    public async Task<PagedResult<SolicitudResumenDTO>> ObtenerBandejaRefugioAsync(int usuarioId, int pagina, int tamano)
    {
        var refugio = await _refugioRepo.ObtenerPorUsuarioIdAsync(usuarioId)
            ?? throw AppException.BadRequest("Usuario no asociado a un refugio.");

        var (items, total) = await _solicitudRepo.ObtenerPorRefugioAsync(refugio.RefugioId, pagina, tamano);

        return new PagedResult<SolicitudResumenDTO>
        {
            Items = items.Select(s => s.ToResumenDto()).ToList(),
            Pagina = pagina,
            TamanoPagina = tamano,
            TotalRegistros = total
        };
    }

    public async Task AprobarSolicitudAsync(int solicitudId, AprobarSolicitudDTO dto, int usuarioId)
    {
        var solicitud = await ValidarPropietarioSolicitudAsync(solicitudId, usuarioId);

        if (solicitud.Estado != 0 && solicitud.Estado != 1) // 0=Enviada, 1=En Revisión
            throw AppException.Conflict("Solo se pueden aprobar solicitudes que estén en estado Enviada o En Revisión.");

        solicitud.Estado = 3; // 3 = Aprobada
        solicitud.Observaciones = dto.Observaciones;
        solicitud.FechaModificacion = DateTime.Now;

        await _solicitudRepo.ActualizarAsync(solicitud);
    }

    public async Task RechazarSolicitudAsync(int solicitudId, RechazarSolicitudDTO dto, int usuarioId)
    {
        if (string.IsNullOrWhiteSpace(dto.MotivoRechazo))
            throw AppException.BadRequest("El motivo de rechazo es obligatorio.");

        var solicitud = await ValidarPropietarioSolicitudAsync(solicitudId, usuarioId);

        if (solicitud.Estado != 0 && solicitud.Estado != 1)
            throw AppException.Conflict("Solo se pueden rechazar solicitudes activas.");

        solicitud.Estado = 4; // 4 = Rechazada
        solicitud.MotivoRechazo = dto.MotivoRechazo;
        solicitud.FechaModificacion = DateTime.Now;

        await _solicitudRepo.ActualizarAsync(solicitud);
    }


    // Helper para verificar que el refugio es dueño de la mascota de la solicitud
    private async Task<SolicitudesAdopcion> ValidarPropietarioSolicitudAsync(int solicitudId, int usuarioId)
    {
        var refugio = await _refugioRepo.ObtenerPorUsuarioIdAsync(usuarioId)
            ?? throw AppException.BadRequest("Usuario no asociado a un refugio.");

        var solicitud = await _solicitudRepo.ObtenerPorIdConMascotaAsync(solicitudId)
            ?? throw AppException.NotFound("Solicitud no encontrada.");

        if (solicitud.Mascota.RefugioId != refugio.RefugioId)
            throw AppException.Forbidden("No tienes permiso para gestionar esta solicitud.");

        return solicitud;
    }
}