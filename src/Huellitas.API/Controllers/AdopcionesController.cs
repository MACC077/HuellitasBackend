using Huellitas.API.Extensions;
using Huellitas.Application.DTOs.Adopciones;
using Huellitas.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Huellitas.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class AdopcionesController : ControllerBase
{
    private readonly IAdopcionService _adopcionService;

    public AdopcionesController(IAdopcionService adopcionService) => _adopcionService = adopcionService;

    /// <summary>
    /// HU-ADO-01: Enviar una solicitud de adopción (Solo Adoptantes).
    /// </summary>
    /// <remarks>
    /// Permite que múltiples usuarios se postulen para la misma mascota.
    /// </remarks>
    /// <response code="201">Solicitud enviada exitosamente.</response>
    /// <response code="409">La mascota no está disponible o ya tienes una solicitud activa.</response>
    [HttpPost(Name = "PostularseAdopcion")]
    [Authorize(Roles = "Adoptante,Administrador")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Postularse([FromBody] CrearSolicitudDTO dto)
    {
        var solicitud = await _adopcionService.PostularseAsync(dto, User.GetUserId());

        return StatusCode(StatusCodes.Status201Created, new
        {
            mensaje = "Solicitud enviada con éxito. El refugio revisará tu perfil.",
            solicitud
        });
    }

    /// <summary>
    /// Obtener las solicitudes enviadas por el adoptante autenticado (Paginado).
    /// </summary>
    [HttpGet("mis-solicitudes", Name = "GetMisSolicitudes")]
    [Authorize(Roles = "Adoptante")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMisSolicitudes([FromQuery] int pagina = 1, [FromQuery] int tamano = 10)
    {
        var result = await _adopcionService.ObtenerMisSolicitudesAsync(User.GetUserId(), pagina, tamano);
        return Ok(result);
    }

    /// <summary>
    /// Obtener la bandeja de solicitudes recibidas por el refugio autenticado (Paginado).
    /// </summary>
    [HttpGet("refugio/bandeja", Name = "GetBandejaRefugio")]
    [Authorize(Roles = "Refugio,Administrador")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBandejaRefugio([FromQuery] int pagina = 1, [FromQuery] int tamano = 10)
    {
        var result = await _adopcionService.ObtenerBandejaRefugioAsync(User.GetUserId(), pagina, tamano);
        return Ok(result);
    }


    /// <summary>
    /// HU-ADO-03: Aprobar una solicitud de adopción (Solo Refugio dueño).
    /// </summary>
    [HttpPatch("{id}/aprobar", Name = "AprobarSolicitud")]
    [Authorize(Roles = "Refugio,Administrador")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Aprobar(int id, [FromBody] AprobarSolicitudDTO dto)
    {
        await _adopcionService.AprobarSolicitudAsync(id, dto, User.GetUserId());
        return Ok(new { mensaje = "Solicitud aprobada correctamente." });
    }

    /// <summary>
    /// HU-ADO-03: Rechazar una solicitud de adopción (Solo Refugio dueño).
    /// </summary>
    [HttpPatch("{id}/rechazar", Name = "RechazarSolicitud")]
    [Authorize(Roles = "Refugio,Administrador")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Rechazar(int id, [FromBody] RechazarSolicitudDTO dto)
    {
        await _adopcionService.RechazarSolicitudAsync(id, dto, User.GetUserId());
        return Ok(new { mensaje = "Solicitud rechazada correctamente." });
    }
}