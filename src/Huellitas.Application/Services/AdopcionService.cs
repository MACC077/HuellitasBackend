using Huellitas.Application.Common;
using Huellitas.Application.Common.Exceptions;
using Huellitas.Application.DTOs.Adopciones;
using Huellitas.Application.Interfaces.Services;
using Huellitas.Application.Mappings;
using Huellitas.Infrastructure.Enums;
using Huellitas.Infrastructure.Interfaces.Repositories;
using Huellitas.Infrastructure.Interfaces.Services;
using Huellitas.Infrastructure.Persistence.Entities;
using System.Text.Json;

namespace Huellitas.Application.Services;

public class AdopcionService : IAdopcionService
{
    private readonly ISolicitudAdopcionRepository _solicitudRepo;
    private readonly IMascotaRepository _mascotaRepo;
    private readonly IRefugioRepository _refugioRepo;
    private readonly IPerfilAdoptanteRepository _perfilRepo;
    private readonly IPdfService _pdfService;

    public AdopcionService(ISolicitudAdopcionRepository solicitudRepo, IMascotaRepository mascotaRepo, IRefugioRepository refugioRepo, IPerfilAdoptanteRepository perfilRepo, IPdfService pdfService)
    {
        _solicitudRepo = solicitudRepo;
        _mascotaRepo = mascotaRepo;
        _refugioRepo = refugioRepo;
        _perfilRepo = perfilRepo;
        _pdfService = pdfService;
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
        solicitud.Estado = (byte)EstadoSolicitudAdopcion.Enviada;
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

        if (solicitud.Estado != (byte)EstadoSolicitudAdopcion.Enviada && solicitud.Estado != (byte)EstadoSolicitudAdopcion.EnRevision)
            throw AppException.Conflict("Solo se pueden aprobar solicitudes que estén en estado Enviada o En Revisión.");

        solicitud.Estado = (byte)EstadoSolicitudAdopcion.Aprobada;
        solicitud.Observaciones = dto.Observaciones;
        solicitud.FechaModificacion = DateTime.Now;

        await _solicitudRepo.ActualizarAsync(solicitud);
    }

    public async Task RechazarSolicitudAsync(int solicitudId, RechazarSolicitudDTO dto, int usuarioId)
    {
        if (string.IsNullOrWhiteSpace(dto.MotivoRechazo))
            throw AppException.BadRequest("El motivo de rechazo es obligatorio.");

        var solicitud = await ValidarPropietarioSolicitudAsync(solicitudId, usuarioId);

        if (solicitud.Estado != (byte)EstadoSolicitudAdopcion.Enviada && solicitud.Estado != (byte)EstadoSolicitudAdopcion.EnRevision)
            throw AppException.Conflict("Solo se pueden rechazar solicitudes activas.");

        solicitud.Estado = (byte)EstadoSolicitudAdopcion.Rechazada;
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

    public async Task FinalizarAdopcionAsync(int solicitudId, int usuarioId)
    {
        //Validar que el refugio es dueño de la solicitud
        var solicitud = await ValidarPropietarioSolicitudAsync(solicitudId, usuarioId);

        //Validar que la solicitud esté Aprobada
        if (solicitud.Estado != (byte)EstadoSolicitudAdopcion.Aprobada)
            throw AppException.Conflict("Solo se pueden finalizar adopciones que hayan sido aprobadas previamente.");

        //Obtener todas las demás solicitudes activas para esta mascota
        var otrasSolicitudes = await _solicitudRepo.ObtenerSolicitudesActivasPorMascotaAsync(solicitud.MascotaId);
        otrasSolicitudes = otrasSolicitudes.Where(s => s.SolicitudId != solicitudId).ToList();

        //Actualizar estados
        solicitud.Estado = (byte)EstadoSolicitudAdopcion.Finalizada; 
        solicitud.FechaModificacion = DateTime.Now;

        foreach (var otra in otrasSolicitudes)
        {
            otra.Estado = (byte)EstadoSolicitudAdopcion.Cerrada;
            otra.Observaciones = "Proceso cerrado automáticamente. La mascota fue adoptada por otro usuario.";
            otra.FechaModificacion = DateTime.Now;
        }

        //Actualizar la Mascota a Adoptada
        var mascota = solicitud.Mascota;
        mascota.Estado = (byte)EstadoMascota.Adoptada;
        mascota.FechaUltimaActualizacion = DateTime.Now;

        //Crear el registro de Adopción (La constancia)
        var nuevaAdopcion = new Adopciones
        {
            SolicitudId = solicitudId,
            FechaFinalizacion = DateTime.Now,
            CodigoConstancia = $"ACT-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
            Activo = true,
            FechaCreacion = DateTime.Now
        };

        //Crear la Bitácora
        var bitacora = new Bitacoras
        {
            MascotaId = mascota.MascotaId,
            Tipo = 3, // 3 = Adopción
            Descripcion = $"Adopción finalizada. Constancia: {nuevaAdopcion.CodigoConstancia}.",
            FechaCreacion = DateTime.Now
        };

        //Ejecutar la transacción
        await _solicitudRepo.EjecutarTransaccionFinalizacionAsync(
            solicitud, otrasSolicitudes, mascota, nuevaAdopcion, bitacora);
    }

    public async Task AgendarVisitaAsync(int solicitudId, AgendarVisitaDTO dto, int usuarioId)
    {
        //Validar que el refugio es dueño de la solicitud
        var solicitud = await ValidarPropietarioSolicitudAsync(solicitudId, usuarioId);

        //Validar estado de la solicitud (debe estar Enviada (0) o EnRevision (1))
        if (solicitud.Estado != (byte)EstadoSolicitudAdopcion.Enviada && solicitud.Estado != (byte)EstadoSolicitudAdopcion.EnRevision)
            throw AppException.Conflict("Solo se puede agendar visita para solicitudes en estado Enviada o En Revisión.");

        //Invalidar visitas anteriores si existen
        var visitaAnterior = await _solicitudRepo.ObtenerVisitaActivaPorSolicitudAsync(solicitudId);
        if (visitaAnterior != null)
        {
            visitaAnterior.Activo = false;
            visitaAnterior.FechaModificacion = DateTime.Now;
            await _solicitudRepo.ActualizarVisitaAsync(visitaAnterior);
        }

        //Crear la nueva visita
        var nuevaVisita = new VisitasEntrevista
        {
            SolicitudId = solicitudId,
            Fecha = dto.Fecha,
            Modalidad = dto.Modalidad,
            Estado = (byte)EstadoVisitaEntrevista.Propuesta,
            Activo = true,
            FechaCreacion = DateTime.Now
        };

        await _solicitudRepo.InsertarVisitaAsync(nuevaVisita);

        //Actualizar estado de la solicitud a Entrevista (2)
        solicitud.Estado = (byte)EstadoSolicitudAdopcion.Entrevista;
        solicitud.FechaModificacion = DateTime.Now;
        await _solicitudRepo.ActualizarAsync(solicitud);
    }

    public async Task ConfirmarVisitaAsync(int solicitudId, ConfirmarVisitaDTO dto, int usuarioId)
    {
        //Validar que la solicitud pertenece al adoptante logueado
        var solicitud = await _solicitudRepo.ObtenerPorIdConMascotaAsync(solicitudId)
            ?? throw AppException.NotFound("Solicitud no encontrada.");

        if (solicitud.UsuarioId != usuarioId)
            throw AppException.Forbidden("No tienes permiso para confirmar esta visita.");

        //Obtener la visita activa
        var visita = await _solicitudRepo.ObtenerVisitaActivaPorSolicitudAsync(solicitudId)
            ?? throw AppException.NotFound("No hay una visita agendada para esta solicitud.");

        if (visita.Estado != (byte)EstadoVisitaEntrevista.Propuesta) // Si ya fue confirmada o reprogramada
            throw AppException.Conflict("Esta visita ya fue gestionada.");

        //Procesar la confirmación
        if (dto.Aceptar)
        {
            visita.Estado = (byte)EstadoVisitaEntrevista.Confirmada; 
        }
        else
        {
            visita.Estado = (byte)EstadoVisitaEntrevista.Reprogramada; // 2 = Reprogramada (el adoptante rechazó la fecha)
                               // Aquí el refugio tendría que agendar otra fecha con el endpoint anterior.
        }

        visita.FechaModificacion = DateTime.Now;
        await _solicitudRepo.ActualizarVisitaAsync(visita);
    }

    public async Task<byte[]> GenerarConstanciaPdfAsync(int solicitudId, int usuarioId)
    {
        var adopcion = await _solicitudRepo.ObtenerAdopcionConDetallesAsync(solicitudId)
            ?? throw AppException.NotFound("Constancia no encontrada o adopción no finalizada.");

        // Validar seguridad: solo el adoptante dueño o el refugio dueño pueden descargarla
        var esAdoptante = adopcion.Solicitud.UsuarioId == usuarioId;

        // Si no es el adoptante, validamos que sea el refugio dueño de la mascota
        if (!esAdoptante)
        {
            var refugio = await _refugioRepo.ObtenerPorUsuarioIdAsync(usuarioId);
            if (refugio == null || adopcion.Solicitud.Mascota.RefugioId != refugio.RefugioId)
                throw AppException.Forbidden("No tienes permiso para descargar esta constancia.");
        }

        // Generar el PDF
        return _pdfService.GenerarConstanciaAdopcion(
            adoptanteNombre: adopcion.Solicitud.Usuario.Nombre,
            adoptanteCorreo: adopcion.Solicitud.Usuario.Correo,
            mascotaNombre: adopcion.Solicitud.Mascota.Nombre,
            refugioNombre: adopcion.Solicitud.Mascota.Refugio.Nombre,
            codigoConstancia: adopcion.CodigoConstancia,
            fechaFinalizacion: adopcion.FechaFinalizacion
        );
    }

    public async Task<DocumentoSoporteDTO> GuardarDocumentoAsync(int solicitudId, CrearDocumentoSoporteDTO dto, int usuarioId)
    {
        // 1. Validar que la solicitud existe y pertenece al adoptante
        var solicitud = await _solicitudRepo.ObtenerPorIdConMascotaAsync(solicitudId)
            ?? throw AppException.NotFound("Solicitud no encontrada.");

        if (solicitud.UsuarioId != usuarioId)
            throw AppException.Forbidden("No tienes permiso para subir documentos a esta solicitud.");

        if (solicitud.Estado >= 5)
            throw AppException.Conflict("No se pueden subir documentos a una solicitud cerrada o finalizada.");

        // 2. Registrar la URL en la base de datos
        var documento = new DocumentosSoporte
        {
            SolicitudId = solicitudId,
            Tipo = dto.Tipo,
            Archivo = dto.ArchivoUrl, // 👈 Usamos la URL del DTO
            FechaCarga = DateTime.Now,
            Activo = true
        };

        await _solicitudRepo.InsertarDocumentoSoporteAsync(documento);

        return new DocumentoSoporteDTO
        {
            DocumentoId = documento.DocumentoId,
            Tipo = documento.Tipo,
            ArchivoUrl = documento.Archivo,
            FechaCarga = documento.FechaCarga
        };
    }

    public async Task<IEnumerable<DocumentoSoporteDTO>> ObtenerDocumentosAsync(int solicitudId, int usuarioId)
    {
        var solicitud = await _solicitudRepo.ObtenerPorIdConMascotaAsync(solicitudId)
            ?? throw AppException.NotFound("Solicitud no encontrada.");

        // Validar seguridad (adoptante dueño o refugio dueño)
        bool esAdoptante = solicitud.UsuarioId == usuarioId;
        bool esRefugio = false;

        if (!esAdoptante)
        {
            var refugio = await _refugioRepo.ObtenerPorUsuarioIdAsync(usuarioId);
            esRefugio = refugio != null && solicitud.Mascota.RefugioId == refugio.RefugioId;
        }

        if (!esAdoptante && !esRefugio)
            throw AppException.Forbidden("No tienes permiso para ver estos documentos.");

        var documentos = await _solicitudRepo.ObtenerDocumentosPorSolicitudAsync(solicitudId);

        return documentos.Select(d => new DocumentoSoporteDTO
        {
            DocumentoId = d.DocumentoId,
            Tipo = d.Tipo,
            ArchivoUrl = d.Archivo,
            FechaCarga = d.FechaCarga
        });
    }

    public async Task MarcarEnRevisionAsync(int solicitudId, int usuarioId)
    {
        // 1. Validar que el refugio es dueño de la solicitud
        var solicitud = await ValidarPropietarioSolicitudAsync(solicitudId, usuarioId);

        // 2. Validar el estado actual (solo se puede revisar si está Enviada)
        // Usamos (byte)EstadoSolicitudAdopcion.Enviada si seguimos usando byte, o simplemente el Enum si ya se migró.
        // Para evitar errores si aún está como byte en la entidad, usamos la comparación con el valor del enum.
        if (solicitud.Estado != (byte)EstadoSolicitudAdopcion.Enviada)
            throw AppException.Conflict("Solo se pueden marcar en revisión las solicitudes que estén en estado 'Enviada'.");

        // 3. Actualizar el estado
        solicitud.Estado = (byte)EstadoSolicitudAdopcion.EnRevision;
        solicitud.FechaModificacion = DateTime.Now;

        await _solicitudRepo.ActualizarAsync(solicitud);
    }

    public async Task<IEnumerable<VisitaResumenDTO>> ObtenerMisVisitasAsync(int usuarioId)
    {
        var visitas = await _solicitudRepo.ObtenerVisitasPorUsuarioAsync(usuarioId);
        return visitas.Select(v => v.ToVisitaDto());
    }

    public async Task<IEnumerable<VisitaResumenDTO>> ObtenerVisitasRefugioAsync(int usuarioId)
    {
        // Obtenemos el RefugioId del usuario logueado
        var refugio = await _refugioRepo.ObtenerPorUsuarioIdAsync(usuarioId)
            ?? throw AppException.BadRequest("Usuario no asociado a un refugio.");

        var visitas = await _solicitudRepo.ObtenerVisitasPorRefugioAsync(refugio.RefugioId);
        return visitas.Select(v => v.ToVisitaDto());
    }
}