using Huellitas.API.Extensions;
using Huellitas.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Huellitas.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class NotificacionesController : ControllerBase
{
    private readonly INotificacionService _service;

    public NotificacionesController(INotificacionService service) => _service = service;

    /// <summary>
    /// TRA-04: Consultar las notificaciones del usuario autenticado (Paginado).
    /// </summary>
    [HttpGet(Name = "GetMisNotificaciones")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Get([FromQuery] int pagina = 1, [FromQuery] int tamano = 10)
        => Ok(await _service.ObtenerMisNotificacionesAsync(User.GetUserId(), pagina, tamano));

    /// <summary>
    /// TRA-04: Marcar una notificación propia como leída.
    /// </summary>
    [HttpPatch("{id}/leer", Name = "MarcarNotificacionLeida")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Leer(long id)
    {
        await _service.MarcarLeidaAsync(id, User.GetUserId());
        return Ok(new { mensaje = "Notificación marcada como leída." });
    }
}
