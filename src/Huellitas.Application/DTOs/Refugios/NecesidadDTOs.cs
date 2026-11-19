namespace Huellitas.Application.DTOs.Refugios;

public class CrearNecesidadDTO
{
    public byte Tipo { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string? MedioDonacion { get; set; } // Ej. "Nequi 3001234567"
}

public class NecesidadDTO
{
    public int NecesidadId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string? MedioDonacion { get; set; }
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaPublicacion { get; set; }
}

public class ActualizarNecesidadDTO
{
    public byte Tipo { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string? MedioDonacion { get; set; }
}