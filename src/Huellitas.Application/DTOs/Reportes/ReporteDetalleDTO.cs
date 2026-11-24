namespace Huellitas.Application.DTOs.Reportes;

/// <summary>
/// Detalle autorizado para el refugio asignado y administradores.
/// </summary>
public class ReporteDetalleDTO : ReporteDetallePublicoDTO
{
    public string? Direccion { get; set; }
    public string? PuntoReferencia { get; set; }
    public string? RefugioAsignado { get; set; }
    public string ReportanteNombre { get; set; } = string.Empty;
    public string? ReportanteTelefono { get; set; }
    public int? MascotaId { get; set; }
}
