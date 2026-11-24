using Huellitas.Application.DTOs.Reportes;
using Huellitas.Infrastructure.Enums;
using Huellitas.Infrastructure.Persistence.Entities;

namespace Huellitas.Application.Mappings;

public static class ReporteMappings
{
    public static ReportesCiudadano ToEntity(this CrearReporteDTO dto, int? usuarioId, DateTime fecha)
    {
        return new ReportesCiudadano
        {
            UsuarioId = usuarioId,
            Tipo = dto.Tipo,
            Descripcion = dto.Descripcion.Trim(),
            FechaReporte = fecha,
            Estado = (byte)EstadoReporte.Recibido,
            Ubicacion_Latitud = dto.Latitud,
            Ubicacion_Longitud = dto.Longitud,
            Ubicacion_Direccion = string.IsNullOrWhiteSpace(dto.Direccion) ? null : dto.Direccion.Trim(),
            Ubicacion_PuntoReferencia = string.IsNullOrWhiteSpace(dto.PuntoReferencia) ? null : dto.PuntoReferencia.Trim(),
            Activo = true,
            FechaCreacion = fecha,
            NumeroReporte = string.Empty // Se asigna en el repositorio.
        };
    }

    public static ReportesCiudadano ToDatosActualizados(this ActualizarReporteDTO dto)
    {
        return new ReportesCiudadano
        {
            Tipo = dto.Tipo,
            Descripcion = dto.Descripcion.Trim(),
            Ubicacion_Latitud = dto.Latitud,
            Ubicacion_Longitud = dto.Longitud,
            Ubicacion_Direccion = string.IsNullOrWhiteSpace(dto.Direccion) ? null : dto.Direccion.Trim(),
            Ubicacion_PuntoReferencia = string.IsNullOrWhiteSpace(dto.PuntoReferencia) ? null : dto.PuntoReferencia.Trim()
        };
    }

    public static ReporteResumenDTO ToResumenDto(this ReportesCiudadano reporte, int refugioId)
    {
        return new ReporteResumenDTO
        {
            ReporteId = reporte.ReporteId,
            NumeroReporte = reporte.NumeroReporte,
            Tipo = ((TipoReporte)reporte.Tipo).ToString(),
            Descripcion = reporte.Descripcion,
            FechaReporte = reporte.FechaReporte,
            Estado = ((EstadoReporte)reporte.Estado).ToString(),
            FotoPrincipal = reporte.Fotos?
                .Where(f => f.Activo)
                .OrderBy(f => f.Id)
                .FirstOrDefault()?.RutaArchivo,
            ReportanteNombre = reporte.Usuario?.Nombre ?? string.Empty,
            AsignadoAMi = reporte.RefugioId == refugioId
        };
    }

    public static ReporteDetalleDTO ToDetalleDto(this ReportesCiudadano reporte)
    {
        return new ReporteDetalleDTO
        {
            ReporteId = reporte.ReporteId,
            NumeroReporte = reporte.NumeroReporte,
            Tipo = ((TipoReporte)reporte.Tipo).ToString(),
            Descripcion = reporte.Descripcion,
            FechaReporte = reporte.FechaReporte,
            Estado = ((EstadoReporte)reporte.Estado).ToString(),
            Latitud = reporte.Ubicacion_Latitud,
            Longitud = reporte.Ubicacion_Longitud,
            Direccion = reporte.Ubicacion_Direccion,
            PuntoReferencia = reporte.Ubicacion_PuntoReferencia,
            RefugioAsignado = reporte.Refugio?.Nombre,
            ReportanteNombre = reporte.Usuario?.Nombre ?? string.Empty,
            ReportanteTelefono = reporte.Usuario?.Telefono,
            MascotaId = reporte.MascotaId,
            Fotos = ObtenerFotos(reporte)
        };
    }

    public static ReporteDetallePublicoDTO ToPublicoDto(this ReportesCiudadano reporte)
    {
        return new ReporteDetallePublicoDTO
        {
            ReporteId = reporte.ReporteId,
            NumeroReporte = reporte.NumeroReporte,
            Tipo = ((TipoReporte)reporte.Tipo).ToString(),
            Descripcion = reporte.Descripcion,
            FechaReporte = reporte.FechaReporte,
            Estado = ((EstadoReporte)reporte.Estado).ToString(),
            Latitud = reporte.Ubicacion_Latitud,
            Longitud = reporte.Ubicacion_Longitud,
            Fotos = ObtenerFotos(reporte)
        };
    }

    public static ReporteMapaDTO ToMapaDto(this ReportesCiudadano reporte)
    {
        return new ReporteMapaDTO
        {
            ReporteId = reporte.ReporteId,
            NumeroReporte = reporte.NumeroReporte,
            Tipo = ((TipoReporte)reporte.Tipo).ToString(),
            Estado = ((EstadoReporte)reporte.Estado).ToString(),
            Latitud = reporte.Ubicacion_Latitud,
            Longitud = reporte.Ubicacion_Longitud
        };
    }

    private static List<string> ObtenerFotos(ReportesCiudadano reporte)
    {
        return reporte.Fotos?
            .Where(f => f.Activo)
            .OrderBy(f => f.Id)
            .Select(f => f.RutaArchivo)
            .ToList() ?? new List<string>();
    }
}
