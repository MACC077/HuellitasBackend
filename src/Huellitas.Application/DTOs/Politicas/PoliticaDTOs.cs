namespace Huellitas.Application.DTOs.Politicas;

public class PoliticaDTO
{
    public int PoliticaId { get; set; }
    public string Version { get; set; } = string.Empty;
    public string Contenido { get; set; } = string.Empty;
    public DateTime FechaPublicacion { get; set; }
    public bool Activo { get; set; }
}

public class CrearPoliticaDTO
{
    public string Version { get; set; } = string.Empty;
    public string Contenido { get; set; } = string.Empty;
    public DateTime? FechaPublicacion { get; set; }
}

public class AceptarPoliticaDTO
{
    /// <summary>Id de la política que el usuario vio. Si se envía, debe coincidir con la activa.</summary>
    public int? PoliticaId { get; set; }

    /// <summary>Versión que el usuario vio. Si se envía, debe coincidir con la activa.</summary>
    public string? Version { get; set; }
}
