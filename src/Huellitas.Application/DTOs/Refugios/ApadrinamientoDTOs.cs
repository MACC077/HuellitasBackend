namespace Huellitas.Application.DTOs.Refugios;

public class CrearApadrinamientoDTO
{
    public int MascotaId { get; set; }
    public string? Observacion { get; set; }
}

public class GestionarApadrinamientoDTO
{
    public bool Aceptar { get; set; }
    public string? Observacion { get; set; }
}

public class ApadrinamientoDTO
{
    public int ApadrinamientoId { get; set; }
    public int MascotaId { get; set; }
    public string MascotaNombre { get; set; } = string.Empty;
    public string? RefugioNombre { get; set; }
    public string AdoptanteNombre { get; set; } = string.Empty;
    public DateTime FechaInicio { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? Observacion { get; set; }
}