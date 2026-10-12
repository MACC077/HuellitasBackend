using Huellitas.Application.DTOs.Auth;
using Huellitas.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Huellitas.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAutenticacionService _auth;

    public AuthController(IAutenticacionService auth) => _auth = auth;

    /// <summary>
    /// HU-USU-02: Iniciar sesión.
    /// </summary>
    [HttpPost("login", Name = "Login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Login([FromBody] LoginDTO dto)
    {
        var response = await _auth.LoginAsync(dto);
        return Ok(response);
    }

    /// <summary>
    /// HU-USU-02: Renovar el Access Token usando un Refresh Token.
    /// </summary>
    [HttpPost("refresh", Name = "RefreshToken")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDTO dto)
    {
        var response = await _auth.RefreshTokenAsync(dto.RefreshToken);
        return Ok(response);
    }

    /// <summary>
    /// HU-USU-02: Cerrar sesión (revoca el Refresh Token).
    /// </summary>
    [HttpPost("logout", Name = "Logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDTO dto)
    {
        await _auth.LogoutAsync(dto.RefreshToken);
        return NoContent();
    }
}