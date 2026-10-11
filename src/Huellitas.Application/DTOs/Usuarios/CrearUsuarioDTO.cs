namespace Huellitas.Application.DTOs.Usuarios;

/// <summary>
/// DTO para el registro de un nuevo usuario (HU-USU-01).
/// Definido en la Figura 25 del documento de tesis.
/// </summary>
public class CrearUsuarioDTO
{
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Contrasena { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public bool AceptaPolitica { get; set; }
}