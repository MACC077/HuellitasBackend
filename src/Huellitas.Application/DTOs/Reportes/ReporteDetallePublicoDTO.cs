namespace Huellitas.Application.DTOs.Reportes;

/// <summary>
/// Información que puede devolverse a un visitante o a un usuario sin relación
/// con el reporte. No incluye datos personales del reportante ni información interna.
/// </summary>
public class ReporteDetallePublicoDTO
{
    public int ReporteId { get; set; }
    public string NumeroReporte { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public DateTime FechaReporte { get; set; }
    public string Estado { get; set; } = string.Empty;
    public decimal Latitud { get; set; }
    public decimal Longitud { get; set; }
    public List<string> Fotos { get; set; } = new();
}
