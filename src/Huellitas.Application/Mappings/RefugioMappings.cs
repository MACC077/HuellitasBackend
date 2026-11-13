using Huellitas.Application.DTOs.Refugios;
using Huellitas.Infrastructure.Persistence.Entities;
using Huellitas.Application.DTOs.Mascotas;

namespace Huellitas.Application.Mappings;

public static class RefugioMappings
{
    public static PerfilRefugioDTO ToPerfilDto(this Refugios r)
    {
        return new PerfilRefugioDTO
        {
            Nombre = r.Nombre,
            Descripcion = r.Descripcion ?? string.Empty,
            Direccion = r.Direccion,
            Telefono = r.Telefono ?? string.Empty,
            RedesSociales = r.RedesSociales ?? string.Empty,
            EstadoVerificacion = r.EstadoVerificacion,
            Requisitos = r.RequisitosAdopcions?.Select(req => new RequisitoDTO
            {
                RequisitoId = req.RequisitoId,
                Descripcion = req.Descripcion,
                EsObligatorio = req.EsObligatorio,
                Orden = req.Orden
            }).ToList() ?? new()
        };
    }

    public static RequisitosAdopcion ToEntity(this RequisitoDTO dto, int refugioId)
    {
        return new RequisitosAdopcion
        {
            RefugioId = refugioId,
            Descripcion = dto.Descripcion,
            EsObligatorio = dto.EsObligatorio,
            Orden = dto.Orden,
            Activo = true,
            FechaCreacion = DateTime.Now
        };
    }

    public static RefugioResumenDTO ToResumenDto(this Refugios r)
    {
        return new RefugioResumenDTO
        {
            RefugioId = r.RefugioId,
            Nombre = r.Nombre,
            Direccion = r.Direccion
        };
    }

    public static RefugioPublicoDTO ToPublicoDto(this Refugios r)
    {
        return new RefugioPublicoDTO
        {
            RefugioId = r.RefugioId,
            Nombre = r.Nombre,
            Descripcion = r.Descripcion ?? string.Empty,
            Direccion = r.Direccion,
            Telefono = r.Telefono,
            Correo = r.Correo,
            RedesSociales = r.RedesSociales,
            Verificado = r.EstadoVerificacion == 1,
            Requisitos = r.RequisitosAdopcions?.Select(req => new RequisitoDTO
            {
                RequisitoId = req.RequisitoId,
                Descripcion = req.Descripcion,
                EsObligatorio = req.EsObligatorio,
                Orden = req.Orden
            }).ToList() ?? new(),
            //Reutilizamos el mapeo de mascotas que ya teníamos
            MascotasDisponibles = r.Mascota?.Select(m => m.ToResumenDto()).ToList() ?? new()
        };

    }
}