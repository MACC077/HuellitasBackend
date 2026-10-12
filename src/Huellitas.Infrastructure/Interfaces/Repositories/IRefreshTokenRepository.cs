using Huellitas.Infrastructure.Persistence.Entities;

namespace Huellitas.Infrastructure.Interfaces.Repositories;

public interface IRefreshTokenRepository
{
    Task InsertarAsync(RefreshTokens token);
    Task ActualizarAsync(RefreshTokens token);
    Task<RefreshTokens?> ObtenerPorHashAsync(string hash);
    Task RevocarTodosDelUsuarioAsync(int usuarioId, string motivo);
    Task<int> ContarActivosDelUsuarioAsync(int usuarioId);
}