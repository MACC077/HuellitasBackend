namespace Huellitas.Application.DTOs.Trazabilidad;

public class PlanSeguimientoDTO
{
    public int PlanId { get; set; }
    public int AdopcionId { get; set; }
    public List<SeguimientoDTO> Seguimientos { get; set; } = new();
}

public class CrearSeguimientoDTO
{
    public string EstadoSalud { get; set; } = string.Empty;
    public string Adaptacion { get; set; } = string.Empty;
    public string Comentarios { get; set; } = string.Empty;
    public List<string> FotosUrls { get; set; } = new();
}

public class SeguimientoDTO
{
    public int SeguimientoId { get; set; }
    public int Numero { get; set; }
    public DateOnly FechaProgramada { get; set; }
    public DateTime? FechaEnvio { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? Resultado { get; set; }
    public List<string> FotosUrls { get; set; } = new();
}
