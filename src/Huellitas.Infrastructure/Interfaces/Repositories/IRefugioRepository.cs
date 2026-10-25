using Huellitas.Infrastructure.Persistence.Entities;

namespace Huellitas.Infrastructure.Interfaces.Repositories;

public interface IRefugioRepository
{
    Task<Refugios?> ObtenerPorUsuarioIdAsync(int usuarioId);
}