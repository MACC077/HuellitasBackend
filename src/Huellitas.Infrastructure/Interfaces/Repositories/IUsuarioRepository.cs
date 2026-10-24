using Huellitas.Infrastructure.Enums;
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
    Task<TokensSeguridad?> ObtenerTokenPorValorAsync(string valor);
    Task ActualizarTokenAsync(TokensSeguridad token);
    Task<TokensSeguridad?> ObtenerTokenActivoPorUsuarioAsync(int usuarioId, TipoToken tipo);
    Task ActualizarContrasenaAsync(int usuarioId, string nuevoHash);
    Task<Usuarios?> ObtenerPerfilCompletoAsync(int usuarioId);
}