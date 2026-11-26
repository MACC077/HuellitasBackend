namespace Huellitas.Application.DTOs.Reportes;

/// <summary>
/// Datos editables de un reporte por parte de un administrador.
/// No incluye estado, refugio, mascota ni reportante: esos campos tienen su propio flujo.
/// </summary>
public class ActualizarReporteDTO
{
    public byte Tipo { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public decimal Latitud { get; set; }
    public decimal Longitud { get; set; }
    public string? Direccion { get; set; }
    public string? PuntoReferencia { get; set; }

    /// <summary>
    /// Si se envían URLs, reemplazan las fotografías actuales.
    /// Si la lista llega vacía, se conservan las existentes.
    /// </summary>
    public List<string> FotosUrls { get; set; } = new();
}
