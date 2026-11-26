namespace Huellitas.Application.DTOs.Administracion;

public class CrearConfiguracionDTO
{
    public string Clave { get; set; } = string.Empty;
    public string Valor { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
}
