using Huellitas.API.Extensions;
using Huellitas.Application.DTOs.Usuarios;
using Huellitas.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Huellitas.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly IAutenticacionService _auth;
    private readonly IUsuarioService _usuarioService;
    public UsuariosController(IAutenticacionService auth, IUsuarioService usuarioService)
    {
        _auth = auth;
        _usuarioService = usuarioService;
    }

    /// <summary>
    /// HU-USU-01: Registro de usuario con verificación de correo.
    /// </summary>
    /// <response code="201">Usuario registrado exitosamente.</response>
    /// <response code="400">Datos de entrada inválidos.</response>
    /// <response code="409">El correo ya está registrado.</response>
    [HttpPost(Name = "RegistrarUsuario")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Registrar([FromBody] CrearUsuarioDTO dto)
    {
        var usuarioId = await _auth.RegistrarAsync(dto);
        var response = new { mensaje = "Usuario registrado. Revisa tu correo.", usuarioId };

        // Devuelve 201 Created con la cabecera 'Location' apuntando al recurso creado.
        return CreatedAtAction(nameof(Registrar), new { id = usuarioId }, response);
    }

    /// <summary>
    /// Confirmar correo usando el token JWT recibido por email.
    /// </summary>
    [HttpGet("confirmar", Name = "ConfirmarCorreo")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ConfirmarCorreo([FromQuery] string token)
    {
        await _auth.ConfirmarCorreoAsync(token);
        return Ok(new { mensaje = "Cuenta verificada exitosamente. Ya puedes iniciar sesión." });
    }

    /// <summary>
    /// Reenviar correo de verificación.
    /// </summary>
    [HttpPost("reenviar-verificacion", Name = "ReenviarCorreoVerificacion")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ReenviarCorreo([FromBody] ReenviarVerificacionDTO dto)
    {
        await _auth.ReenviarCorreoVerificacionAsync(dto.Correo);
        return Ok(new { mensaje = "Correo de verificación reenviado exitosamente." });
    }

    /// <summary>
    /// Solicitar recuperación de contraseña.
    /// </summary>
    [HttpPost("recuperar", Name = "SolicitarRecuperacion")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Recuperar([FromBody] SolicitarRecuperacionDTO dto)
    {
        await _auth.SolicitarRecuperacionAsync(dto.Correo);
        return Ok(new { mensaje = "Si el correo está registrado, recibirás un código de recuperación." });
    }

    /// <summary>
    /// Restablecer contraseña con token.
    /// </summary>
    [HttpPost("restablecer", Name = "RestablecerContrasena")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Restablecer([FromBody] RestablecerContrasenaDTO dto)
    {
        await _auth.RestablecerContrasenaAsync(dto.Token, dto.NuevaContrasena);
        return Ok(new { mensaje = "Contraseña restablecida exitosamente. Ya puedes iniciar sesión." });
    }

    /// <summary>
    /// Obtener los datos del perfil del usuario autenticado.
    /// </summary>
    [HttpGet("perfil", Name = "GetPerfil")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPerfil()
    {
        var result = await _usuarioService.ObtenerPerfilAsync(User.GetUserId());
        return Ok(result);
    }

    /// <summary>
    /// Actualizar la información básica del perfil (nombre, teléfono, foto).
    /// </summary>
    [HttpPut("perfil", Name = "UpdatePerfil")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdatePerfil([FromBody] ActualizarPerfilDTO dto)
    {
        await _usuarioService.ActualizarPerfilAsync(User.GetUserId(), dto);
        return Ok(new { mensaje = "Perfil actualizado correctamente." });
    }

    /// <summary>
    /// Cambiar la contraseña del usuario autenticado (requiere la actual).
    /// </summary>
    [HttpPut("cambiar-password", Name = "CambiarPassword")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CambiarPassword([FromBody] CambiarPasswordDTO dto)
    {
        await _usuarioService.CambiarPasswordAsync(User.GetUserId(), dto);
        return Ok(new { mensaje = "Contraseña actualizada. Por seguridad, inicia sesión de nuevo." });
    }

    /// <summary>
    /// Cambiar el correo electrónico (requiere verificación posterior).
    /// </summary>
    [HttpPut("cambiar-correo", Name = "CambiarCorreo")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CambiarCorreo([FromBody] CambiarCorreoDTO dto)
    {
        await _usuarioService.CambiarCorreoAsync(User.GetUserId(), dto);
        return Ok(new { mensaje = "Correo actualizado. Revisa tu nueva bandeja para verificarlo." });
    }

    /// <summary>
    /// Desactivar la cuenta (soft delete) del usuario autenticado.
    /// </summary>
    [HttpPatch("desactivar", Name = "DesactivarCuenta")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Desactivar([FromBody] DesactivarCuentaDTO dto)
    {
        await _usuarioService.DesactivarCuentaAsync(User.GetUserId(), dto);
        return Ok(new { mensaje = "Cuenta desactivada correctamente." });
    }

    /// <summary>
    /// Obtener el perfil de adoptante del usuario autenticado.
    /// </summary>
    [HttpGet("perfil-adoptante", Name = "GetPerfilAdoptante")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPerfilAdoptante()
    {
        var result = await _usuarioService.ObtenerPerfilAdoptanteAsync(User.GetUserId());
        return Ok(result);
    }

    /// <summary>
    /// Crear o actualizar el perfil de adoptante.
    /// </summary>
    [HttpPut("perfil-adoptante", Name = "UpdatePerfilAdoptante")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdatePerfilAdoptante([FromBody] ActualizarPerfilAdoptanteDTO dto)
    {
        await _usuarioService.ActualizarPerfilAdoptanteAsync(User.GetUserId(), dto);
        return Ok(new { mensaje = "Perfil de adoptante actualizado correctamente." });
    }
}
