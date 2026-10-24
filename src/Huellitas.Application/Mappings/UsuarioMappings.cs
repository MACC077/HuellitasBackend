using Huellitas.Application.DTOs.Usuarios;
using Huellitas.Infrastructure.Enums;
using Huellitas.Infrastructure.Persistence.Entities;

namespace Huellitas.Application.Mappings;

/// <summary>
/// Mapeos manuales para la entidad Usuarios (HU-USU-01).
/// </summary>
public static class UsuarioMappings
{
    /// <summary>
    /// Convierte un CrearUsuarioDTO a una entidad Usuarios.
    /// Los campos calculados (ContrasenaHash, Estado, RolId, etc.) se asignan en el Service.
    /// </summary>
    public static Usuarios ToEntity(this CrearUsuarioDTO dto)
    {
        return new Usuarios
        {
            Nombre = dto.Nombre,
            Correo = dto.Correo,
            Telefono = dto.Telefono,
            FechaRegistro = DateTime.Now,
            Activo = true,
            FechaCreacion = DateTime.Now,
            IntentosFallidos = 0
            // ContrasenaHash, Estado, RolId los asigna el UsuarioService. Ya que vienen de la lógica de negocio y no del DTO.
        };
    }

    /// <summary>
    /// Mapea la entidad Usuario al DTO de perfil, incluyendo datos del consentimiento.
    /// </summary>
    public static UsuarioPerfilDTO ToPerfilDto(this Usuarios usuario)
    {
        var consentimiento = usuario.ConsentimientosDatos?
            .OrderByDescending(c => c.FechaAceptacion)
            .FirstOrDefault();

        return new UsuarioPerfilDTO
        {
            Id = usuario.UsuarioId,
            Nombre = usuario.Nombre,
            Correo = usuario.Correo,
            Telefono = usuario.Telefono,
            FotoUrl = usuario.FotoUrl,
            Estado = usuario.Estado == (byte)EstadoUsuario.Activo ? "Activo" : "Pendiente/Inactivo",
            FechaRegistro = usuario.FechaRegistro,
            PoliticaAceptada = consentimiento?.Politica?.Version,
            FechaAceptacionPolitica = consentimiento?.FechaAceptacion
        };
    }

    /// <summary>
    /// Mapea la entidad PerfilesAdoptante al DTO, calculando el porcentaje de completitud.
    /// </summary>
    public static PerfilAdoptanteDTO ToDto(this PerfilesAdoptante? perfil)
    {
        if (perfil == null)
            return new PerfilAdoptanteDTO { PorcentajeCompletitud = 0 };

        int total = 6;
        int completados = 0;
        if (perfil.TipoVivienda.HasValue) completados++;
        if (!string.IsNullOrEmpty(perfil.EspacioDisponible)) completados++;
        if (perfil.PresenciaDeNinos.HasValue) completados++;
        if (perfil.OtrasMascotas.HasValue) completados++;
        if (!string.IsNullOrEmpty(perfil.TiempoDisponible)) completados++;
        if (perfil.ExperienciaPrevia.HasValue) completados++;

        return new PerfilAdoptanteDTO
        {
            TipoVivienda = perfil.TipoVivienda switch
            {
                0 => "Casa",
                1 => "Apartamento",
                2 => "Finca",
                _ => null
            },
            EspacioDisponible = perfil.EspacioDisponible,
            PresenciaDeNinos = perfil.PresenciaDeNinos,
            OtrasMascotas = perfil.OtrasMascotas,
            TiempoDisponible = perfil.TiempoDisponible,
            ExperienciaPrevia = perfil.ExperienciaPrevia,
            PorcentajeCompletitud = (int)Math.Round((double)completados / total * 100)
        };
    }
}