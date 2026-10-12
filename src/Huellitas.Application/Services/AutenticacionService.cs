using Huellitas.Application.Common.Exceptions;
using Huellitas.Application.DTOs.Usuarios;
using Huellitas.Application.Interfaces.Services;
using Huellitas.Application.Mappings;
using Huellitas.Infrastructure.Enums;
using Huellitas.Infrastructure.Interfaces.Repositories;
using Huellitas.Infrastructure.Interfaces.Services;
using Huellitas.Infrastructure.Persistence.Entities;

namespace Huellitas.Application.Services;

public class AutenticacionService : IAutenticacionService
{
    private readonly IUsuarioRepository _repo;
    private readonly IHashContrasena _hash;
    private readonly IProveedorTokens _tokens;
    private readonly ICorreoService _correo;
    private readonly ITokenValidador _validador;

    public AutenticacionService(
        IUsuarioRepository repo,
        IHashContrasena hash,
        IProveedorTokens tokens,
        ICorreoService correo,
        ITokenValidador validador)
    {
        _repo = repo;
        _hash = hash;
        _tokens = tokens;
        _correo = correo;
        _validador = validador;
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

        var tokenValor = _tokens.GenerarTokenSeguro();
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

        if (!_validador.ValidarToken(token))
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
            await _repo.ActualizarTokenAsync(tokenAnterior);
        }

        var nuevoToken = _tokens.GenerarTokenSeguro();
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
}
