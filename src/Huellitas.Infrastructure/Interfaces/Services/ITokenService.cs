using Huellitas.Infrastructure.Persistence.Entities;

namespace Huellitas.Infrastructure.Interfaces.Services;

public interface ITokenService
{
    // Generación
    string GenerarAccessToken(Usuarios usuario, string nombreRol);
    string GenerarRefreshToken();
    string GenerarTokenVerificacion(int usuarioId);

    // Validación
    bool ValidarToken(string token);

    // Hashing determinístico para almacenamiento
    string HashearToken(string token);
}