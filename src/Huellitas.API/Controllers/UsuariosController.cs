using Huellitas.Application.DTOs.Usuarios;
using Huellitas.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Huellitas.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly IAutenticacionService _auth;
    public UsuariosController(IAutenticacionService auth) => _auth = auth;

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
}