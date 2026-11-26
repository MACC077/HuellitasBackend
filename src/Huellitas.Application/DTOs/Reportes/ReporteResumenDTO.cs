namespace Huellitas.Application.DTOs.Reportes;

public class ReporteResumenDTO
{
    public int ReporteId { get; set; }
    public string NumeroReporte { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public DateTime FechaReporte { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? FotoPrincipal { get; set; }
    public string ReportanteNombre { get; set; } = string.Empty;
    public bool AsignadoAMi { get; set; }
}
