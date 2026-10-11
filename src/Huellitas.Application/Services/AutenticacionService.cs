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

    public AutenticacionService(
        IUsuarioRepository repo,
        IHashContrasena hash,
        IProveedorTokens tokens,
        ICorreoService correo)
    {
        _repo = repo;
        _hash = hash;
        _tokens = tokens;
        _correo = correo;
    }

    public async Task<int> RegistrarAsync(CrearUsuarioDTO dto)
    {
        if (await _repo.ExisteCorreoAsync(dto.Correo))
            throw AppException.Conflict("El correo ya está registrado.");

        var usuario = dto.ToEntity();
        usuario.ContrasenaHash = _hash.Hashear(dto.Contrasena);
        usuario.Estado = (byte)EstadoUsuario.PendienteVerificacion;
        usuario.RolId = await _repo.ObtenerRolIdPorNombreAsync("Adoptante");
        usuario.FechaRegistro = DateTime.Now;
        usuario.Activo = true;
        usuario.FechaCreacion = DateTime.Now;
        usuario.IntentosFallidos = 0;

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
            VersionAceptada = "1.0",
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

    public Task<bool> ConfirmarCorreoAsync(string token) => throw new NotImplementedException();
    public Task ReenviarCorreoVerificacionAsync(string correo) => throw new NotImplementedException();
}
