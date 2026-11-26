using Huellitas.API.Extensions;
using Huellitas.Application.DTOs.Politicas;
using Huellitas.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Huellitas.API.Controllers;

/// <summary>
/// Términos y política de tratamiento de datos con control de versión.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class PoliticasController : ControllerBase
{
    private readonly IPoliticaService _politicaService;

    public PoliticasController(IPoliticaService politicaService) => _politicaService = politicaService;

    /// <summary>Obtener la política de datos vigente (Público).</summary>
    [HttpGet("activa", Name = "GetPoliticaActiva")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PoliticaDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetActiva()
    {
        return Ok(await _politicaService.ObtenerActivaAsync());
    }

    /// <summary>Listar el historial de versiones de la política (Solo Administrador).</summary>
    [HttpGet(Name = "GetPoliticas")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(IEnumerable<PoliticaDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTodas()
    {
        return Ok(await _politicaService.ObtenerTodasAsync());
    }

    /// <summary>
    /// Publicar una nueva versión de la política. La versión anterior queda inactiva y
    /// los usuarios deberán aceptarla de nuevo.
    /// </summary>
    [HttpPost(Name = "CrearPolitica")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Crear([FromBody] CrearPoliticaDTO dto)
    {
        var id = await _politicaService.CrearAsync(dto, User.GetUserId());

        return StatusCode(StatusCodes.Status201Created, new
        {
            mensaje = "Política publicada. Los usuarios deberán aceptarla nuevamente.",
            politicaId = id
        });
    }

    /// <summary>
    /// Aceptar la política vigente. Si la versión cambió desde que el usuario la leyó,
    /// se devuelve 409 para que recargue el texto.
    /// </summary>
    [HttpPost("aceptar", Name = "AceptarPolitica")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Aceptar([FromBody] AceptarPoliticaDTO dto)
    {
        await _politicaService.AceptarAsync(User.GetUserId(), dto);
        return Ok(new { mensaje = "Política aceptada." });
    }
}
