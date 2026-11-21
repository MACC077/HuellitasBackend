using Huellitas.Application.DTOs.Mascotas;
using Huellitas.Application.DTOs.Refugios;
using Huellitas.Infrastructure.Enums;
using Huellitas.Infrastructure.Persistence.Entities;
using System.Text.Json;

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
            RedesSociales = DeserializarRedesSociales(r.RedesSociales),
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
            RedesSociales = DeserializarRedesSociales(r.RedesSociales),
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

    public static ApadrinamientoDTO ToDto(this Apadrinamientos a)
    {
        return new ApadrinamientoDTO
        {
            ApadrinamientoId = a.ApadrinamientoId,
            MascotaId = a.MascotaId,
            MascotaNombre = a.Mascota?.Nombre ?? "",
            RefugioNombre = a.Mascota?.Refugio?.Nombre,
            AdoptanteNombre = a.Usuario?.Nombre ?? "",
            FechaInicio = a.FechaInicio.ToDateTime(TimeOnly.MinValue),
            Estado = ((EstadoApadrinamiento)a.Estado).ToString(),
            Observacion = a.Observacion
        };
    }

    public static string? SerializarRedesSociales(Dictionary<string, string>? redes)
    {
        return redes == null ? null : JsonSerializer.Serialize(redes);
    }

    private static Dictionary<string, string>? DeserializarRedesSociales(string? redesSociales)
    {
        if (string.IsNullOrWhiteSpace(redesSociales))
            return null;

        try
        {
            // Intentamos parsear el JSON nuevo
            return JsonSerializer.Deserialize<Dictionary<string, string>>(redesSociales);
        }
        catch (JsonException)
        {
            // Si falla (es texto plano viejo), lo adaptamos al formato diccionario
            return new Dictionary<string, string>
            {
                { "redes", redesSociales } // Lo guardamos bajo una llave genérica
            };
        }
    }
}