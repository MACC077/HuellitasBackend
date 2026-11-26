using Huellitas.API.Extensions;
using Huellitas.Application.DTOs.Refugios;
using Huellitas.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Huellitas.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class ApadrinamientosController : ControllerBase
{
    private readonly IApadrinamientoService _service;

    public ApadrinamientosController(IApadrinamientoService service) => _service = service;

    /// <summary>
    /// HU-REF-03: Solicitar ser padrino de una mascota (Solo Adoptantes/Usuarios externos).
    /// </summary>
    [HttpPost(Name = "SolicitarApadrinamiento")]
    [Authorize(Roles = "Adoptante")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Solicitar([FromBody] CrearApadrinamientoDTO dto)
    {
        var id = await _service.SolicitarApadrinamientoAsync(User.GetUserId(), dto);
        return Ok(new { mensaje = "Solicitud de apadrinamiento enviada.", apadrinamientoId = id });
    }

    /// <summary>
    /// HU-REF-03: Aprobar o rechazar una solicitud de apadrinamiento (Solo Refugio).
    /// </summary>
    [HttpPatch("{id}/gestionar", Name = "GestionarApadrinamiento")]
    [Authorize(Roles = "Refugio")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Gestionar(int id, [FromBody] GestionarApadrinamientoDTO dto)
    {
        await _service.GestionarApadrinamientoAsync(User.GetUserId(), id, dto);
        return Ok(new { mensaje = dto.Aceptar ? "Apadrinamiento aceptado." : "Apadrinamiento rechazado." });
    }

    /// <summary>
    /// HU-REF-03: Ver mis apadrinamientos
    /// </summary>
    [HttpGet("mis-apadrinamientos", Name = "GetMisApadrinamientos")]
    [Authorize(Roles = "Adoptante")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMisApadrinamientos([FromQuery] int pagina = 1, [FromQuery] int tamano = 10)
    {
        var result = await _service.ObtenerMisApadrinamientosAsync(User.GetUserId(), pagina, tamano);
        return Ok(result);
    }

    /// <summary>
    /// HU-REF-03: Ver los apadrinamientos solicitados a mis mascotas
    /// </summary>
    [HttpGet("refugio", Name = "GetApadrinamientosRefugio")]
    [Authorize(Roles = "Refugio")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetApadrinamientosRefugio([FromQuery] int pagina = 1, [FromQuery] int tamano = 10)
    {
        var result = await _service.ObtenerApadrinamientosRefugioAsync(User.GetUserId(), pagina, tamano);
        return Ok(result);
    }

    /// <summary>
    /// HU-REF-03: Cancelar un apadrinamiento activo (Solo el usuario que lo creó).
    /// </summary>
    [HttpPatch("{id}/cancelar", Name = "CancelarApadrinamiento")]
    [Authorize(Roles = "Adoptante")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Cancelar(int id)
    {
        await _service.CancelarApadrinamientoAsync(User.GetUserId(), id);
        return Ok(new { mensaje = "Apadrinamiento cancelado correctamente." });
    }
}