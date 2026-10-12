using Huellitas.Application.Common.Exceptions;
using Huellitas.Application.DTOs.Auth;
using Huellitas.Application.DTOs.Usuarios;
using Huellitas.Application.Interfaces.Services;
using Huellitas.Application.Mappings;
using Huellitas.Infrastructure.Enums;
using Huellitas.Infrastructure.Interfaces.Repositories;
using Huellitas.Infrastructure.Interfaces.Services;
using Huellitas.Infrastructure.Persistence.Entities;
using Huellitas.Infrastructure.Services;

namespace Huellitas.Application.Services;

public class AutenticacionService : IAutenticacionService
{
    private const int MAX_INTENTOS = 5;
    private const int MINUTOS_BLOQUEO = 15;

    private readonly IUsuarioRepository _repo;
    private readonly IRefreshTokenRepository _refreshRepo;
    private readonly IHashContrasena _hash;
    private readonly ICorreoService _correo;
    private readonly ITokenService _tokenService;

    public AutenticacionService(
        IUsuarioRepository repo,
        IRefreshTokenRepository refreshRepo,
        IHashContrasena hash,
        ICorreoService correo,
        ITokenService tokenService)
    {
        _repo = repo;
        _refreshRepo = refreshRepo;
        _hash = hash;
        _correo = correo;
        _tokenService = tokenService;
    }

    public async Task<int> RegistrarAsync(CrearUsuarioDTO dto)
    {
        if (await _repo.ExisteCorreoAsync(dto.Correo))
            throw AppException.Conflict("El correo ya está registrado.");

        var usuario = dto.ToEntity();
        usuario.ContrasenaHash = _hash.Hashear(dto.Contrasena);
        usuario.Estado = (byte)EstadoUsuario.PendienteVerificacion;
        usuario.RolId = await _repo.ObtenerRolIdPorNombreAsync("Adoptante");


        if (usuario.RolId == 0)
            throw AppException.BadRequest("Configuración del sistema incompleta: no existe el rol Adoptante.");

        await _repo.InsertarAsync(usuario);

        var politicaId = await _repo.ObtenerPoliticaActivaIdAsync();
        if (politicaId == 0)
            throw AppException.BadRequest("Configuración del sistema incompleta: no hay política de datos activa.");

        await _repo.InsertarConsentimientoAsync(new ConsentimientosDatos
        {
            UsuarioId = usuario.UsuarioId,
            PoliticaId = politicaId,
            VersionAceptada = "1.0", //No debe estar quemado debe acualizarse, con lo que trae la BD, pero por ahora lo dejamos así
            FechaAceptacion = DateTime.Now,
            Activo = true
        });

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
        return usuario.UsuarioId;
    }

    public async Task<bool> ConfirmarCorreoAsync(string token) 
    {

        if (!_tokenService.ValidarToken(token))
            throw AppException.BadRequest("Token inválido o expirado.");

        var tokenDb = await _repo.ObtenerTokenPorValorAsync(token);
        if (tokenDb == null || tokenDb.Usado || tokenDb.FechaExpiracion < DateTime.Now)
            throw AppException.BadRequest("Token inválido o ya usado.");


        var usuario = await _repo.ObtenerPorIdAsync(tokenDb.UsuarioId);
        if (usuario == null)
            throw AppException.NotFound("Usuario no encontrado.");

 
        if (usuario.Estado != (byte)EstadoUsuario.PendienteVerificacion)
            throw AppException.Conflict("La cuenta ya está verificada o bloqueada.");


        usuario.Estado = (byte)EstadoUsuario.Activo;
        usuario.FechaModificacion = DateTime.Now;
        await _repo.ActualizarAsync(usuario);


        tokenDb.Usado = true;
        tokenDb.Activo = false;
        await _repo.ActualizarTokenAsync(tokenDb);

        //Siempre retorna true, por seguridad
        return true;
    }
    public async Task ReenviarCorreoVerificacionAsync(string correo) 
    {

        var usuario = await _repo.ObtenerPorCorreoAsync(correo);

        if (usuario == null) return;

        if (usuario.Estado == (byte)EstadoUsuario.Activo) return;

        var tokenAnterior = await _repo.ObtenerTokenActivoPorUsuarioAsync(
            usuario.UsuarioId, TipoToken.VerificacionCorreo);

        if (tokenAnterior != null)
        {
            tokenAnterior.Usado = true;
            tokenAnterior.Activo = false;
            await _repo.ActualizarTokenAsync(tokenAnterior);
        }

        var nuevoToken = _tokenService.GenerarTokenVerificacion(usuario.UsuarioId);
        await _repo.InsertarTokenAsync(new TokensSeguridad
        {
            UsuarioId = usuario.UsuarioId,
            Tipo = (byte)TipoToken.VerificacionCorreo,
            Valor = nuevoToken,
            FechaExpiracion = DateTime.Now.AddHours(24),
            Usado = false,
            Activo = true,
            FechaCreacion = DateTime.Now
        });

        await _correo.EnviarCorreoVerificacionAsync(usuario.Correo, usuario.Nombre, nuevoToken);
    }

    public async Task<TokenResponseDTO> LoginAsync(LoginDTO dto)
    {
        // 1. Buscar usuario
        var usuario = await _repo.ObtenerPorCorreoAsync(dto.Correo);
        if (usuario == null)
            throw AppException.Unauthorized("Credenciales inválidas.");

        // 2. Verificar bloqueo temporal
        if (usuario.FechaBloqueo.HasValue && usuario.FechaBloqueo > DateTime.Now)
        {
            var minutosRestantes = (int)(usuario.FechaBloqueo.Value - DateTime.Now).TotalMinutes + 1;
            throw AppException.Forbidden($"Cuenta bloqueada temporalmente. Intenta de nuevo en {minutosRestantes} minuto(s).");
        }

        // 3. Verificar contraseña
        if (!_hash.Verificar(dto.Contrasena, usuario.ContrasenaHash))
        {
            await _repo.IncrementarIntentosFallidosAsync(usuario.UsuarioId);

            if (usuario.IntentosFallidos + 1 >= MAX_INTENTOS)
            {
                await _repo.BloquearUsuarioAsync(usuario.UsuarioId, DateTime.Now.AddMinutes(MINUTOS_BLOQUEO));
                
                throw AppException.Forbidden($"Cuenta bloqueada por {MINUTOS_BLOQUEO} minutos por múltiples intentos fallidos.");
            }

            throw AppException.Unauthorized("Credenciales inválidas.");
        }

        // 4. Verificar estado activo
        if (usuario.Estado == (byte)EstadoUsuario.PendienteVerificacion)
            throw AppException.Forbidden("Debes confirmar tu correo antes de iniciar sesión.");

        if (usuario.Estado != (byte)EstadoUsuario.Activo)
            throw AppException.Forbidden("La cuenta no está activa. Contacta al administrador.");

        // 5. Resetear intentos fallidos
        await _repo.ResetearIntentosFallidosAsync(usuario.UsuarioId);

        // 6. Obtener nombre del rol
        var nombreRol = await _repo.ObtenerNombreRolAsync(usuario.RolId) ?? "Adoptante";

        // 7. Generar tokens
        var accessToken = _tokenService.GenerarAccessToken(usuario, nombreRol);
        var refreshToken = _tokenService.GenerarRefreshToken();

        // 8. Guardar refresh token (hasheado)
        var diasRefresh = 7;
        await _refreshRepo.InsertarAsync(new RefreshTokens
        {
            UsuarioId = usuario.UsuarioId,
            TokenHash = _tokenService.HashearToken(refreshToken),
            Expira = DateTime.Now.AddDays(diasRefresh),
            EstaRevocado = false,
            Creado = DateTime.Now
        });

        // 9. Retornar
        return new TokenResponseDTO
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiraEnSegundos = 15 * 60,
            Nombre = usuario.Nombre,
            Rol = nombreRol
        };
    }

    public async Task<TokenResponseDTO> RefreshTokenAsync(string refreshToken)
    {
        // 1. Hashear y buscar
        var hash = _hash.Hashear(refreshToken);
        var tokenDb = await _refreshRepo.ObtenerPorHashAsync(hash);

        if (tokenDb == null)
            throw AppException.Unauthorized("Refresh token inválido.");

        // 2. Detección de reutilización → revocar TODA la cadena
        if (tokenDb.EstaRevocado)
        {
            await _refreshRepo.RevocarTodosDelUsuarioAsync(tokenDb.UsuarioId, "Reutilización detectada - sesión comprometida");
            throw AppException.Unauthorized("Token revocado. Inicia sesión nuevamente.");
        }

        // 3. Verificar expiración
        if (tokenDb.Expira < DateTime.Now)
            throw AppException.Unauthorized("Refresh token expirado. Inicia sesión nuevamente.");

        // 4. Verificar estado del usuario
        var usuario = tokenDb.Usuario;
        if (usuario.Estado != (byte)EstadoUsuario.Activo)
            throw AppException.Forbidden("La cuenta no está activa.");

        // 5. Rotación: revocar el actual
        var nuevoRefreshToken = _tokenService.GenerarRefreshToken();
        var nuevoHash = _hash.Hashear(nuevoRefreshToken);

        tokenDb.EstaRevocado = true;
        tokenDb.RevocadoEn = DateTime.Now;
        tokenDb.ReemplazadoPor = nuevoHash;
        await _refreshRepo.ActualizarAsync(tokenDb);

        // 6. Insertar el nuevo
        var diasRefresh = 7;
        await _refreshRepo.InsertarAsync(new RefreshTokens
        {
            UsuarioId = usuario.UsuarioId,
            TokenHash = nuevoHash,
            Expira = DateTime.Now.AddDays(diasRefresh),
            EstaRevocado = false,
            Creado = DateTime.Now
        });

        // 7. Generar nuevo access token
        var nombreRol = await _repo.ObtenerNombreRolAsync(usuario.RolId) ?? "Adoptante";
        var nuevoAccessToken = _tokenService.GenerarAccessToken(usuario, nombreRol);

        return new TokenResponseDTO
        {
            AccessToken = nuevoAccessToken,
            RefreshToken = nuevoRefreshToken,
            ExpiraEnSegundos = 15 * 60,
            Nombre = usuario.Nombre,
            Rol = nombreRol
        };
    }

    public async Task LogoutAsync(string refreshToken)
    {
        var hash = _tokenService.HashearToken(refreshToken);
        var tokenDb = await _refreshRepo.ObtenerPorHashAsync(hash);

        if (tokenDb == null || tokenDb.EstaRevocado) return;

        tokenDb.EstaRevocado = true;
        tokenDb.RevocadoEn = DateTime.Now;
        tokenDb.ReemplazadoPor = "Logout";
        await _refreshRepo.ActualizarAsync(tokenDb);
    }
}
