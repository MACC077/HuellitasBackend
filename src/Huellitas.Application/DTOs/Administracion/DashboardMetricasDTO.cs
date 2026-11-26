namespace Huellitas.Application.DTOs.Administracion;

public class DashboardMetricasDTO
{
    public int TotalAdopcionesExitosas { get; set; }
    public int TotalMascotasDisponibles { get; set; }
    public int TotalUsuariosRegistrados { get; set; }
    public int TotalRefugiosVerificados { get; set; }
    public int TotalReportesCiudadanos { get; set; }
    public double TiempoPromedioAdopcionDias { get; set; }

    /// <summary>
    /// Reportes agrupados por estado. Si la consulta incluye estadoReporte,
    /// la lista contiene únicamente ese estado.
    /// </summary>
    public List<ReportePorEstadoDTO> ReportesPorEstado { get; set; } = new();
}
