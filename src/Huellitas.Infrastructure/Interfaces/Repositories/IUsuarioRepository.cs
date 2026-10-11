using Huellitas.Infrastructure.Persistence.Entities;

namespace Huellitas.Infrastructure.Interfaces.Repositories;

public interface IUsuarioRepository
{
    Task<bool> ExisteCorreoAsync(string correo);
    Task<Usuarios> InsertarAsync(Usuarios usuario);
    Task<Usuarios?> ObtenerPorCorreoAsync(string correo);
    Task<Usuarios?> ObtenerPorIdAsync(int id);
    Task ActualizarAsync(Usuarios usuario);
    Task<int> ObtenerRolIdPorNombreAsync(string nombreRol);
    Task<int> ObtenerPoliticaActivaIdAsync();
    Task InsertarTokenAsync(TokensSeguridad token);
    Task InsertarConsentimientoAsync(ConsentimientosDatos consentimiento);
}