using Huellitas.API.Extensions;
using Huellitas.Application.DTOs.Adopciones;
using Huellitas.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Huellitas.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Roles = "Adoptante,Administrador")]
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
    [Authorize(Roles = "Adoptante")]
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
}