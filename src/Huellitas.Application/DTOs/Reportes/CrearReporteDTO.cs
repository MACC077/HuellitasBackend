namespace Huellitas.Application.DTOs.Reportes;

public class CrearReporteDTO
{
    public byte Tipo { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public decimal Latitud { get; set; }
    public decimal Longitud { get; set; }
    public string? Direccion { get; set; }
    public string? PuntoReferencia { get; set; }
    public List<string> FotosUrls { get; set; } = new();
}
