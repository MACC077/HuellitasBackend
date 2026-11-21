namespace Huellitas.Application.DTOs.Usuarios;

public class RegistroRefugioPublicoDTO
{
    // Datos del Usuario (Encargado)
    public string NombreEncargado { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Contrasena { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public bool AceptaPolitica { get; set; }

    // Datos del Refugio
    public string NombreRefugio { get; set; } = string.Empty;
    public string Nit { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? RedesSociales { get; set; } // Texto plano o JSON, se maneja en el servicio
}