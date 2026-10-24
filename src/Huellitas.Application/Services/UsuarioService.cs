using Huellitas.Application.Common.Exceptions;
using Huellitas.Application.DTOs.Usuarios;
using Huellitas.Application.Interfaces.Services;
using Huellitas.Application.Mappings;
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
    private readonly IPerfilAdoptanteRepository _perfilRepo;

    public UsuarioService(IUsuarioRepository repo, 
        IHashContrasena hash, 
        ICorreoService correo,
        ITokenService tokenService, 
        IRefreshTokenRepository refreshRepo,
        IPerfilAdoptanteRepository perfilRepo)
    {
        _repo = repo; 
        _hash = hash; 
        _correo = correo; 
        _tokenService = tokenService; 
        _refreshRepo = refreshRepo;
        _perfilRepo = perfilRepo;
    }

    public async Task<UsuarioPerfilDTO> ObtenerPerfilAsync(int usuarioId)
    {
        var usuario = await _repo.ObtenerPerfilCompletoAsync(usuarioId)
            ?? throw AppException.NotFound("Usuario no encontrado.");

        var consentimiento = usuario.ConsentimientosDatos?.OrderByDescending(c => c.FechaAceptacion).FirstOrDefault();

        return usuario.ToPerfilDto();
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

    public async Task<PerfilAdoptanteDTO> ObtenerPerfilAdoptanteAsync(int usuarioId)
    {
        var perfil = await _perfilRepo.ObtenerPorUsuarioIdAsync(usuarioId);

        if (perfil == null)
        {
            return new PerfilAdoptanteDTO { PorcentajeCompletitud = 0 };
        }

        return new PerfilAdoptanteDTO
        {
            TipoVivienda = perfil.TipoVivienda switch
            {
                0 => "Casa",
                1 => "Apartamento",
                2 => "Finca",
                _ => null
            },
            EspacioDisponible = perfil.EspacioDisponible,
            PresenciaDeNinos = perfil.PresenciaDeNinos,
            OtrasMascotas = perfil.OtrasMascotas,
            TiempoDisponible = perfil.TiempoDisponible,
            ExperienciaPrevia = perfil.ExperienciaPrevia,
            PorcentajeCompletitud = CalcularCompletitud(perfil)
        };
    }

    /// <summary>
    /// Crea o actualiza el perfil de adoptante del usuario autenticado (Operación Upsert).
    /// </summary>
    /// <remarks>
    /// Dado que el perfil de adoptante tiene una relación 1 a 1 con el usuario, 
    /// se implementa un PUT idempotente que actúa como "crear o actualizar". 
    /// Esto simplifica el consumo desde el frontend, ya que el cliente no necesita 
    /// consultar previamente si el recurso existe para decidir si hacer un POST o un PUT.
    /// </remarks>
    public async Task ActualizarPerfilAdoptanteAsync(int usuarioId, ActualizarPerfilAdoptanteDTO dto)
    {
        var perfil = await _perfilRepo.ObtenerPorUsuarioIdAsync(usuarioId);

        if (perfil == null)
        {
            // Crear nuevo perfil
            perfil = new PerfilesAdoptante
            {
                UsuarioId = usuarioId,
                TipoVivienda = (byte?)dto.TipoVivienda,
                EspacioDisponible = dto.EspacioDisponible,
                PresenciaDeNinos = dto.PresenciaDeNinos,
                OtrasMascotas = dto.OtrasMascotas,
                TiempoDisponible = dto.TiempoDisponible,
                ExperienciaPrevia = dto.ExperienciaPrevia,
                Activo = true,
                FechaCreacion = DateTime.Now
            };
            await _perfilRepo.InsertarAsync(perfil);
        }
        else
        {
            // Actualizar existente
            perfil.TipoVivienda = (byte?)dto.TipoVivienda;
            perfil.EspacioDisponible = dto.EspacioDisponible;
            perfil.PresenciaDeNinos = dto.PresenciaDeNinos;
            perfil.OtrasMascotas = dto.OtrasMascotas;
            perfil.TiempoDisponible = dto.TiempoDisponible;
            perfil.ExperienciaPrevia = dto.ExperienciaPrevia;
            perfil.FechaModificacion = DateTime.Now;

            await _perfilRepo.ActualizarAsync(perfil);
        }
    }

    private int CalcularCompletitud(PerfilesAdoptante p)
    {
        int total = 6;
        int completados = 0;

        if (p.TipoVivienda.HasValue) completados++;
        if (!string.IsNullOrEmpty(p.EspacioDisponible)) completados++;
        if (p.PresenciaDeNinos.HasValue) completados++;
        if (p.OtrasMascotas.HasValue) completados++;
        if (!string.IsNullOrEmpty(p.TiempoDisponible)) completados++;
        if (p.ExperienciaPrevia.HasValue) completados++;

        return (int)Math.Round((double)completados / total * 100);
    }
}