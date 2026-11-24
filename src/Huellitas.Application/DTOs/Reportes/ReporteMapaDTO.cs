namespace Huellitas.Application.DTOs.Reportes;

public class ReporteMapaDTO
{
    public int ReporteId { get; set; }
    public string NumeroReporte { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public decimal Latitud { get; set; }
    public decimal Longitud { get; set; }
}
