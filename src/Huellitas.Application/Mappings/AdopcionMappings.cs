using Huellitas.Application.DTOs.Adopciones;
using Huellitas.Infrastructure.Persistence.Entities;

namespace Huellitas.Application.Mappings;

public static class AdopcionMappings
{
    /// <summary>
    /// Convierte un DTO a la entidad. El servicio se encarga de asignar 
    /// los campos de auditoría (UsuarioId, Codigo, Fechas, Estado, Activo).
    /// </summary>
    public static SolicitudesAdopcion ToEntity(this CrearSolicitudDTO dto, string respuestasSerializadas)
    {
        return new SolicitudesAdopcion
        {
            MascotaId = dto.MascotaId,
            RespuestasFormulario = respuestasSerializadas
        };
    }

    /// <summary>
    /// Convierte la entidad al DTO de respuesta descriptivo.
    /// </summary>
    public static SolicitudCreadaDTO ToSolicitudCreadaDto(this SolicitudesAdopcion solicitud, string mascotaNombre)
    {
        return new SolicitudCreadaDTO
        {
            SolicitudId = solicitud.SolicitudId,
            Codigo = solicitud.Codigo,
            MascotaNombre = mascotaNombre,
            FechaEnvio = solicitud.FechaEnvio,
            Estado = "Enviada" // Al ser recién creada, siempre es Enviada
        };
    }


    public static SolicitudResumenDTO ToResumenDto(this SolicitudesAdopcion s)
    {
        return new SolicitudResumenDTO
        {
            SolicitudId = s.SolicitudId,
            Codigo = s.Codigo,
            MascotaNombre = s.Mascota?.Nombre ?? "",
            MascotaFoto = s.Mascota?.Multimedia?.FirstOrDefault(m => m.EsPrincipal)?.Archivo,
            AdoptanteNombre = s.Usuario?.Nombre ?? "",
            FechaEnvio = s.FechaEnvio,
            Estado = s.Estado switch
            {
                0 => "Enviada",
                1 => "En Revisión",
                2 => "Entrevista",
                3 => "Aprobada",
                4 => "Rechazada",
                5 => "Finalizada",
                6 => "Cancelada",
                _ => "Desconocido"
            }
        };
    }
}