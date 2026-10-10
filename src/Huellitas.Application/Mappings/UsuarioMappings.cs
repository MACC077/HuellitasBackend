using Huellitas.Application.DTOs.Usuarios;
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
            Telefono = dto.Telefono
            // ContrasenaHash, Estado, RolId, FechaRegistro,
            // Activo, FechaCreacion → los asigna el UsuarioService.
        };
    }
}