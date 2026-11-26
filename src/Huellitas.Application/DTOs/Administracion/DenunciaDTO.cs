namespace Huellitas.Application.DTOs.Administracion;

public class DenunciaDTO
{
    public int DenunciaId { get; set; }
    public string Denunciante { get; set; } = string.Empty;
    public string TipoContenido { get; set; } = string.Empty;
    public int IdContenido { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? Resultado { get; set; }
    public DateTime Fecha { get; set; }
}
