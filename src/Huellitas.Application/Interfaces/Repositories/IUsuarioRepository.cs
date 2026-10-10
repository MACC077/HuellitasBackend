using Huellitas.Infrastructure.Persistence.Entities;

namespace Huellitas.Application.Interfaces.Repositories;

public interface IUsuarioRepository
{
    Task<bool> ExisteCorreoAsync(string correo);
    Task<Usuarios> InsertarAsync(Usuarios usuario);
    Task<Usuarios?> ObtenerPorCorreoAsync(string correo);
    Task<Usuarios?> ObtenerPorIdAsync(int id);
    Task ActualizarAsync(Usuarios usuario);
}