namespace Huellitas.Infrastructure.Interfaces.Services;

/// <summary>
/// Genera tokens de verificación de correo y recuperación de contraseña.
/// </summary>
public interface IProveedorTokens
{
    string GenerarTokenSeguro();
}