namespace Huellitas.Infrastructure.Enums;

/// <summary>
/// Tipo de token de seguridad. Corresponde a TokensSeguridad.Tipo (TINYINT).
/// Valores según script SQL.
/// </summary>
public enum TipoToken : byte
{
    VerificacionCorreo = 0,
    RecuperacionContrasena = 1
}