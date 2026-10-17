namespace Huellitas.Application.DTOs.Usuarios;

public class RestablecerContrasenaDTO
{
    public string Token { get; set; } = string.Empty;
    public string NuevaContrasena { get; set; } = string.Empty;
}