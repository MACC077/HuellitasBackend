namespace Huellitas.Application.DTOs.Adopciones;

public class AprobarSolicitudDTO
{
    public string? Observaciones { get; set; }
}

public class RechazarSolicitudDTO
{
    public string MotivoRechazo { get; set; } = string.Empty;
}