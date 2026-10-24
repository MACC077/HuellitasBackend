namespace Huellitas.Application.DTOs.Usuarios;

public class PerfilAdoptanteDTO
{
    public string? TipoVivienda { get; set; }
    public string? EspacioDisponible { get; set; }
    public bool? PresenciaDeNinos { get; set; }
    public bool? OtrasMascotas { get; set; }
    public string? TiempoDisponible { get; set; }
    public bool? ExperienciaPrevia { get; set; }
    public int PorcentajeCompletitud { get; set; }
}

public class ActualizarPerfilAdoptanteDTO
{
    public int? TipoVivienda { get; set; } // 0: Casa, 1: Apartamento, 2: Finca
    public string? EspacioDisponible { get; set; }
    public bool? PresenciaDeNinos { get; set; }
    public bool? OtrasMascotas { get; set; }
    public string? TiempoDisponible { get; set; }
    public bool? ExperienciaPrevia { get; set; }
}