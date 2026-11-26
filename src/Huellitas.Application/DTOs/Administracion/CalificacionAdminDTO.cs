namespace Huellitas.Application.DTOs.Administracion;

public class CalificacionAdminDTO
{
    public int CalificacionId { get; set; }
    public int RefugioId { get; set; }
    public string RefugioNombre { get; set; } = string.Empty;
    public string AdoptanteNombre { get; set; } = string.Empty;
    public byte Puntaje { get; set; }
    public string? Comentario { get; set; }
    public string EstadoModeracion { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public bool Activo { get; set; }
}

public class ModerarCalificacionDTO
{
    /// <summary>0 = Publicada, 1 = Oculta.</summary>
    public byte EstadoModeracion { get; set; }
}
