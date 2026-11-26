namespace Huellitas.Application.DTOs.Auth;

public class TokenResponseDTO
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int ExpiraEnSegundos { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;

    /// <summary>
    /// Indica si el usuario debe aceptar la política de datos vigente antes de continuar.
    /// </summary>
    public bool RequiereAceptarPolitica { get; set; }
    public string? VersionPoliticaActual { get; set; }
    public int? PoliticaIdActual { get; set; }
}