namespace Huellitas.Infrastructure.Interfaces.Services;

/// <summary>
/// Valida tokens JWT (firma, expiración, issuer, audience).
/// </summary>
public interface ITokenValidador
{
    bool ValidarToken(string token);
}