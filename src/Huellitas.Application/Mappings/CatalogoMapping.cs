using Huellitas.Application.DTOs.Catalogos;
using Huellitas.Infrastructure.Persistence.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Huellitas.Application.Mappings
{
    public static class CatalogoMapping 
    {
        // 1. Especies
        public static EspecieDTO ToDto(this Especies entity)
        {
            return new EspecieDTO
            {
                Id = entity.EspecieId,
                Nombre = entity.Nombre,
                Activo = entity.Activo
            };
        }

        public static IEnumerable<EspecieDTO> ToDtoList(this IEnumerable<Especies> entities)
            => entities.Select(e => e.ToDto()).OrderBy(e => e.Id);

        // 2. Razas
        public static RazaDTO ToDto(this Razas entity)
        {
            return new RazaDTO
            {
                Id = entity.RazaId,
                Nombre = entity.Nombre,
                Activo = entity.Activo,
                EspecieId = entity.EspecieId
            };
        }

        public static IEnumerable<RazaDTO> ToDtoList(this IEnumerable<Razas> entities)
            => entities.Select(r => r.ToDto()).OrderBy(r => r.Id);

        // 3. Tamaños
        public static TamanoDTO ToDto(this Tamanos entity)
        {
            return new TamanoDTO
            {
                Id = entity.TamanoId,
                Nombre = entity.Nombre,
                Activo = entity.Activo
            };
        }

        public static IEnumerable<TamanoDTO> ToDtoList(this IEnumerable<Tamanos> entities)
            => entities.Select(t => t.ToDto()).OrderBy(t => t.Id);

        // 4. Colores
        public static ColorDTO ToDto(this Colores entity)
        {
            return new ColorDTO
            {
                Id = entity.ColorId,
                Nombre = entity.Nombre,
                Activo = entity.Activo
            };
        }

        public static IEnumerable<ColorDTO> ToDtoList(this IEnumerable<Colores> entities)
            => entities.Select(c => c.ToDto()).OrderBy(c => c.Id);

        // 5. Temperamentos
        public static TemperamentoDTO ToDto(this Temperamentos entity)
        {
            return new TemperamentoDTO
            {
                Id = entity.TemperamentoId,
                Nombre = entity.Nombre,
                Activo = entity.Activo
            };
        }

        public static IEnumerable<TemperamentoDTO> ToDtoList(this IEnumerable<Temperamentos> entities)
            => entities.Select(t => t.ToDto()).OrderBy(t => t.Id);
    }
}
