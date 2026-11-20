using Huellitas.API.Extensions;
using Huellitas.Application.DTOs.Trazabilidad;
using Huellitas.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Huellitas.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class SeguimientosController : ControllerBase
{
    private readonly ISeguimientoService _service;

    public SeguimientosController(ISeguimientoService service) => _service = service;

    /// <summary>
    /// TRA-05: Consultar el plan de seguimiento de una adopción.
    /// </summary>
    /// <remarks>
    /// Disponible para el adoptante dueño, el refugio asociado y administradores.
    /// </remarks>
    [HttpGet("adopcion/{id}/plan", Name = "GetPlanSeguimiento")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPlan(int id)
        => Ok(await _service.ObtenerPlanAsync(id, User.GetUserId(), User.IsInRole("Administrador")));

    /// <summary>
    /// TRA-05: Enviar el reporte de un seguimiento propio (Solo el adoptante dueño).
    /// </summary>
    [HttpPost("{id}/reporte", Name = "EnviarReporteSeguimiento")]
    [Authorize(Roles = "Adoptante")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> EnviarReporte(int id, [FromBody] CrearSeguimientoDTO dto)
    {
        await _service.EnviarReporteAsync(id, dto, User.GetUserId());
        return Ok(new { mensaje = "Reporte enviado correctamente." });
    }

    /// <summary>
    /// TRA-05: Evaluar un seguimiento enviado (Solo el refugio asociado).
    /// </summary>
    [HttpPatch("{id}/evaluar", Name = "EvaluarSeguimiento")]
    [Authorize(Roles = "Refugio")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Evaluar(int id, [FromQuery] byte resultado)
    {
        await _service.EvaluarSeguimientoAsync(id, resultado, User.GetUserId());
        return Ok(new { mensaje = "Seguimiento evaluado." });
    }

    /// <summary>
    /// TRA-06: Procesar las alertas diarias de seguimientos vencidos (Solo Administrador).
    /// </summary>
    [HttpPost("procesar-alertas", Name = "ProcesarAlertasSeguimiento")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ProcesarAlertas()
    {
        await _service.ProcesarAlertasDiariasAsync();
        return Ok(new { mensaje = "Alertas procesadas." });
    }
}
