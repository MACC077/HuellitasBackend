using Huellitas.Application.Common.Exceptions;
using Huellitas.Application.DTOs.Usuarios;
using Huellitas.Application.Interfaces.Services;
using Huellitas.Infrastructure.Enums;
using Huellitas.Infrastructure.Interfaces.Repositories;
using Huellitas.Infrastructure.Interfaces.Services;
using Huellitas.Infrastructure.Persistence.Entities;

namespace Huellitas.Application.Services;

public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _repo;
    private readonly IHashContrasena _hash;
    private readonly ICorreoService _correo;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenRepository _refreshRepo;

    public UsuarioService(IUsuarioRepository repo, 
        IHashContrasena hash, 
        ICorreoService correo,
        ITokenService tokenService, 
        IRefreshTokenRepository refreshRepo)
    {
        _repo = repo; 
        _hash = hash; 
        _correo = correo; 
        _tokenService = tokenService; 
        _refreshRepo = refreshRepo;
    }

    public async Task<UsuarioPerfilDTO> ObtenerPerfilAsync(int usuarioId)
    {
        var usuario = await _repo.ObtenerPerfilCompletoAsync(usuarioId)
            ?? throw AppException.NotFound("Usuario no encontrado.");

        var consentimiento = usuario.ConsentimientosDatos?.OrderByDescending(c => c.FechaAceptacion).FirstOrDefault();

        return new UsuarioPerfilDTO
        {
            Id = usuario.UsuarioId,
            Nombre = usuario.Nombre,
            Correo = usuario.Correo,
            Telefono = usuario.Telefono,
            FotoUrl = usuario.FotoUrl,
            Estado = usuario.Estado == (byte)EstadoUsuario.Activo ? "Activo" : "Pendiente/Inactivo",
            FechaRegistro = usuario.FechaRegistro,
            PoliticaAceptada = consentimiento?.Politica?.Version,
            FechaAceptacionPolitica = consentimiento?.FechaAceptacion
        };
    }

    public async Task ActualizarPerfilAsync(int usuarioId, ActualizarPerfilDTO dto)
    {
        var usuario = await _repo.ObtenerPorIdAsync(usuarioId) ?? throw AppException.NotFound("Usuario no encontrado.");

        usuario.Nombre = dto.Nombre;
        usuario.Telefono = dto.Telefono;
        usuario.FotoUrl = dto.FotoUrl;
        usuario.FechaModificacion = DateTime.Now;

        await _repo.ActualizarAsync(usuario);
    }

    public async Task CambiarPasswordAsync(int usuarioId, CambiarPasswordDTO dto)
    {
        var usuario = await _repo.ObtenerPorIdAsync(usuarioId) ?? throw AppException.NotFound("Usuario no encontrado.");

        if (!_hash.Verificar(dto.PasswordActual, usuario.ContrasenaHash))
            throw AppException.BadRequest("La contraseña actual es incorrecta.");

        usuario.ContrasenaHash = _hash.Hashear(dto.PasswordNueva);
        usuario.FechaModificacion = DateTime.Now;
        await _repo.ActualizarAsync(usuario);

        // Cerrar sesión en todos los dispositivos
        await _refreshRepo.RevocarTodosDelUsuarioAsync(usuarioId, "Cambio de contraseña");
    }

    public async Task CambiarCorreoAsync(int usuarioId, CambiarCorreoDTO dto)
    {
        var usuario = await _repo.ObtenerPorIdAsync(usuarioId) ?? throw AppException.NotFound("Usuario no encontrado.");

        if (!_hash.Verificar(dto.PasswordActual, usuario.ContrasenaHash))
            throw AppException.BadRequest("La contraseña es incorrecta.");

        if (await _repo.ExisteCorreoAsync(dto.NuevoCorreo))
            throw AppException.Conflict("El nuevo correo ya está en uso.");

        usuario.Correo = dto.NuevoCorreo;
        usuario.Estado = (byte)EstadoUsuario.PendienteVerificacion; // Bloqueado hasta verificar
        usuario.FechaModificacion = DateTime.Now;
        await _repo.ActualizarAsync(usuario);

        // Generar token y enviar correo de verificación al NUEVO correo
        var tokenValor = _tokenService.GenerarTokenVerificacion(usuario.UsuarioId);
        await _repo.InsertarTokenAsync(new TokensSeguridad
        {
            UsuarioId = usuario.UsuarioId,
            Tipo = (byte)TipoToken.VerificacionCorreo,
            Valor = tokenValor,
            FechaExpiracion = DateTime.Now.AddHours(24),
            Usado = false,
            Activo = true,
            FechaCreacion = DateTime.Now
        });

        await _correo.EnviarCorreoVerificacionAsync(usuario.Correo, usuario.Nombre, tokenValor);
    }

    public async Task DesactivarCuentaAsync(int usuarioId, DesactivarCuentaDTO dto)
    {
        var usuario = await _repo.ObtenerPorIdAsync(usuarioId) ?? throw AppException.NotFound("Usuario no encontrado.");

        if (!_hash.Verificar(dto.PasswordActual, usuario.ContrasenaHash))
            throw AppException.BadRequest("La contraseña es incorrecta.");

        usuario.Activo = false;
        usuario.Estado = (byte)EstadoUsuario.Desactivado;
        usuario.FechaModificacion = DateTime.Now;
        await _repo.ActualizarAsync(usuario);

        await _refreshRepo.RevocarTodosDelUsuarioAsync(usuarioId, "Cuenta desactivada");
    }
}