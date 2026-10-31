namespace Huellitas.Application.DTOs.Adopciones;

public class SolicitudResumenDTO
{
    public int SolicitudId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string MascotaNombre { get; set; } = string.Empty;
    public string? MascotaFoto { get; set; }
    public string AdoptanteNombre { get; set; } = string.Empty;
    public DateTime FechaEnvio { get; set; }
    public string Estado { get; set; } = string.Empty;
}