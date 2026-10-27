using Huellitas.Application.Common.Exceptions;
using Huellitas.Application.DTOs.Adopciones;
using Huellitas.Application.Interfaces.Services;
using Huellitas.Infrastructure.Interfaces.Repositories;
using Huellitas.Infrastructure.Persistence.Entities;
using Huellitas.Application.Mappings;
using System.Text.Json;

namespace Huellitas.Application.Services;

public class AdopcionService : IAdopcionService
{
    private readonly ISolicitudAdopcionRepository _solicitudRepo;
    private readonly IMascotaRepository _mascotaRepo;

    public AdopcionService(ISolicitudAdopcionRepository solicitudRepo, IMascotaRepository mascotaRepo)
    {
        _solicitudRepo = solicitudRepo;
        _mascotaRepo = mascotaRepo;
    }

    public async Task<SolicitudCreadaDTO> PostularseAsync(CrearSolicitudDTO dto, int usuarioId)
    {
        //Validar que la mascota exista y esté Disponible (0) o En Proceso (1)
        var mascota = await _mascotaRepo.ObtenerPorIdAsync(dto.MascotaId)
            ?? throw AppException.NotFound("Mascota no encontrada.");

        if (mascota.Estado == 2 || mascota.Estado == 3) // Adoptada o No Disponible
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
        if (mascota.Estado == 0)
        {
            await _solicitudRepo.ActualizarEstadoMascotaAsync(dto.MascotaId, 1);
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
}