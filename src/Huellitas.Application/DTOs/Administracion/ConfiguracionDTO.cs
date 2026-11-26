namespace Huellitas.Application.DTOs.Administracion;

public class ConfiguracionDTO
{
    public int ConfiguracionId { get; set; }
    public string Clave { get; set; } = string.Empty;
    public string Valor { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool Activo { get; set; }
}
