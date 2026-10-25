using Huellitas.Application.DTOs.Mascotas;
using Huellitas.Infrastructure.Persistence.Entities;

namespace Huellitas.Application.Mappings;

public static class MascotaMappings
{
    /// <summary>
    /// Convierte una entidad Mascota al DTO de resumen.
    /// </summary>
    public static MascotaResumenDTO ToResumenDto(this Mascotas m)
    {
        return new MascotaResumenDTO
        {
            Id = m.MascotaId,
            Nombre = m.Nombre,
            Especie = m.Especie?.Nombre ?? string.Empty,
            Raza = m.Raza?.Nombre,
            Sexo = m.Sexo == 0 ? "Macho" : "Hembra",
            EdadAproximada = m.EdadAproximada ?? 0,
            Estado = m.Estado switch
            {
                0 => "Disponible",
                1 => "En Proceso",
                2 => "Adoptada",
                _ => "No Disponible"
            },
            FotoPrincipal = m.Multimedia?.FirstOrDefault(f => f.EsPrincipal)?.Archivo,
            FechaCreacion = m.FechaCreacion
        };
    }

    public static IEnumerable<MascotaResumenDTO> ToResumenDtoList(this IEnumerable<Mascotas> entities)
        => entities.Select(m => m.ToResumenDto());

    /// <summary>
    /// Crea una nueva entidad Mascota a partir del DTO de creación.
    /// Los campos de auditoría y estado se asignan en el servicio.
    /// </summary>
    public static Mascotas ToEntity(this CrearMascotaDTO dto)
    {
        return new Mascotas
        {
            EspecieId = dto.EspecieId,
            RazaId = dto.RazaId,
            TamanoId = dto.TamanoId,
            ColorId = dto.ColorId,
            Nombre = dto.Nombre,
            Sexo = dto.Sexo,
            EdadAproximada = dto.EdadAproximada,
            Descripcion = dto.Descripcion,
            Esterilizada = dto.Esterilizada,
            Procedencia = dto.Procedencia
        };
    }

    /// <summary>
    /// Actualiza una entidad Mascota existente con los datos del DTO.
    /// </summary>
    public static void UpdateFromDto(this Mascotas mascota, ActualizarMascotaDTO dto)
    {
        mascota.Nombre = dto.Nombre;
        mascota.RazaId = dto.RazaId;
        mascota.TamanoId = dto.TamanoId;
        mascota.ColorId = dto.ColorId;
        mascota.Sexo = dto.Sexo;
        mascota.EdadAproximada = dto.EdadAproximada;
        mascota.Descripcion = dto.Descripcion;
        mascota.Esterilizada = dto.Esterilizada;
    }
}