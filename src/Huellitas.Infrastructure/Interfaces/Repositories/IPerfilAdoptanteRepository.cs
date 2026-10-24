using Huellitas.Infrastructure.Persistence.Entities;

namespace Huellitas.Infrastructure.Interfaces.Repositories;

public interface IPerfilAdoptanteRepository
{
    Task<PerfilesAdoptante?> ObtenerPorUsuarioIdAsync(int usuarioId);
    Task<PerfilesAdoptante> InsertarAsync(PerfilesAdoptante perfil);
    Task ActualizarAsync(PerfilesAdoptante perfil);
}