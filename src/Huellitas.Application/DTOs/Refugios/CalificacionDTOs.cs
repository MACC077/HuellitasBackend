namespace Huellitas.Application.DTOs.Refugios;

public class CrearCalificacionDTO
{
    public int AdopcionId { get; set; }
    public byte Puntaje { get; set; } // 1 a 5
    public string? Comentario { get; set; }
}

public class CalificacionDTO
{
    public int CalificacionId { get; set; }
    public string AdoptanteNombre { get; set; } = string.Empty;
    public byte Puntaje { get; set; }
    public string? Comentario { get; set; }
    public DateTime Fecha { get; set; }
}

public class ActualizarCalificacionDTO
{
    public byte Puntaje { get; set; }
    public string? Comentario { get; set; }
}