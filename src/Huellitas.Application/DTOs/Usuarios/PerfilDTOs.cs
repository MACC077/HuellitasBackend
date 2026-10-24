namespace Huellitas.Application.DTOs.Usuarios;

public class UsuarioPerfilDTO
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? FotoUrl { get; set; }
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; }

    // Datos de la Ley 1581
    public string? PoliticaAceptada { get; set; }
    public DateTime? FechaAceptacionPolitica { get; set; }
}

public class ActualizarPerfilDTO
{
    public string Nombre { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? FotoUrl { get; set; }
}

public class CambiarPasswordDTO
{
    public string PasswordActual { get; set; } = string.Empty;
    public string PasswordNueva { get; set; } = string.Empty;
}

public class CambiarCorreoDTO
{
    public string NuevoCorreo { get; set; } = string.Empty;
    public string PasswordActual { get; set; } = string.Empty; // Seguridad
}

public class DesactivarCuentaDTO
{
    public string PasswordActual { get; set; } = string.Empty;
}