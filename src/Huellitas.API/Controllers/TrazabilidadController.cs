using Huellitas.API.Extensions;
using Huellitas.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Huellitas.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class TrazabilidadController : ControllerBase
{
    private readonly ITrazabilidadService _trazabilidadService;

    public TrazabilidadController(ITrazabilidadService trazabilidadService)
        => _trazabilidadService = trazabilidadService;

    /// <summary>
    /// TRA-01: Consultar la línea de tiempo de una solicitud de adopción.
    /// </summary>
    /// <remarks>
    /// Disponible para el adoptante dueño de la solicitud, el refugio asociado y administradores.
    /// </remarks>
    [HttpGet("solicitudes/{id}", Name = "GetLineaTiempo")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLineaTiempo(int id)
    {
        var esAdmin = User.IsInRole("Administrador");
        return Ok(await _trazabilidadService.ObtenerLineaTiempoAsync(id, User.GetUserId(), esAdmin));
    }

    /// <summary>
    /// TRA-02: Consultar el historial (bitácora) público de una mascota.
    /// </summary>
    /// <remarks>
    /// Endpoint anónimo. Solo expone el tipo de evento, la descripción y la fecha.
    /// </remarks>
    [HttpGet("mascotas/{id}", Name = "GetHistorialMascota")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHistorialMascota(int id)
        => Ok(await _trazabilidadService.ObtenerHistorialMascotaAsync(id));

    /// <summary>
    /// TRA-03: Consultar los registros de auditoría (Solo Administrador).
    /// </summary>
    [HttpGet("auditoria", Name = "GetAuditoria")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAuditoria([FromQuery] int pagina = 1, [FromQuery] int tamano = 20)
        => Ok(await _trazabilidadService.ObtenerAuditoriaAsync(pagina, tamano));
}
