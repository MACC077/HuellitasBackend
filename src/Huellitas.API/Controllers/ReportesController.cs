using Huellitas.API.Extensions;
using Huellitas.Application.DTOs.Reportes;
using Huellitas.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Huellitas.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class ReportesController : ControllerBase
{
    private readonly IReporteService _reporteService;

    public ReportesController(IReporteService reporteService) => _reporteService = reporteService;

    /// <summary>
    /// Registrar un reporte ciudadano de un animal en situación de calle, herido o maltratado.
    /// </summary>
    /// <remarks>
    /// Es un endpoint público: si el solicitante está autenticado, el reporte se asocia
    /// a su usuario; de lo contrario se registra como anónimo. Las fotografías aceptan
    /// URLs externas (http/https), no solo las cargadas en /api/v1/Archivos.
    /// </remarks>
    /// <response code="201">Reporte registrado exitosamente.</response>
    /// <response code="400">Datos inválidos o fotografías no permitidas.</response>
    /// <response code="429">Demasiadas solicitudes desde esta IP. Intenta más tarde.</response>
    [HttpPost(Name = "CrearReporte")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingExtensions.PoliticaReportes)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Crear([FromBody] CrearReporteDTO dto)
    {
        var result = await _reporteService.CrearReporteAsync(dto, User.GetUserId());

        return StatusCode(StatusCodes.Status201Created, new
        {
            mensaje = "Reporte registrado.",
            id = result.ReporteId,
            numeroReporte = result.NumeroReporte
        });
    }

    /// <summary>
    /// Bandeja del refugio autenticado: reportes disponibles y reportes ya asignados a su refugio.
    /// </summary>
    /// <response code="200">Lista paginada de reportes.</response>
    /// <response code="400">Paginación inválida o usuario sin refugio asociado.</response>
    /// <response code="403">El refugio no está activo.</response>
    [HttpGet("bandeja", Name = "GetBandejaReportes")]
    [Authorize(Roles = "Refugio,Administrador")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetBandeja([FromQuery] int pagina = 1, [FromQuery] int tamano = 10)
    {
        var result = await _reporteService.ObtenerBandejaRefugioAsync(User.GetUserId(), pagina, tamano);
        return Ok(result);
    }

    /// <summary>
    /// Consultar el detalle de un reporte.
    /// </summary>
    /// <remarks>
    /// El público recibe únicamente información no personal. El refugio asignado
    /// y los administradores reciben el detalle autorizado.
    /// </remarks>
    /// <response code="200">Detalle del reporte.</response>
    /// <response code="404">El reporte no existe o no está disponible públicamente.</response>
    [HttpGet("{id:int}", Name = "GetDetalleReporte")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ReporteDetallePublicoDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDetalle(int id)
    {
        var result = await _reporteService.ObtenerDetalleAsync(
            id,
            User.GetUserId(),
            User.IsInRole("Administrador"));

        return Ok(result);
    }

    /// <summary>
    /// Tomar un reporte disponible para iniciar su atención.
    /// </summary>
    /// <response code="200">Reporte asignado al refugio autenticado.</response>
    /// <response code="409">El reporte ya fue tomado o no está en estado Recibido.</response>
    [HttpPatch("{id:int}/tomar", Name = "TomarReporte")]
    [Authorize(Roles = "Refugio,Administrador")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Tomar(int id)
    {
        await _reporteService.TomarReporteAsync(id, User.GetUserId(), User.IsInRole("Administrador"));
        return Ok(new { mensaje = "Reporte tomado. Ahora está en atención." });
    }

    /// <summary>
    /// Cambiar el estado de un reporte.
    /// </summary>
    /// <remarks>
    /// El refugio autenticado solo puede operar sobre reportes asignados a él; un
    /// administrador puede hacerlo sobre cualquier reporte, incluso sin refugio asignado.
    /// Transiciones permitidas: EnAtencion → Rescatado, NoEncontrado o Cerrado.
    /// El motivo es obligatorio para NoEncontrado y Cerrado.
    /// </remarks>
    /// <response code="200">Estado actualizado.</response>
    /// <response code="400">Estado inválido o motivo faltante.</response>
    /// <response code="403">El reporte no está asignado al refugio autenticado.</response>
    /// <response code="409">Transición no permitida o reporte ya convertido en mascota.</response>
    [HttpPatch("{id:int}/estado", Name = "CambiarEstadoReporte")]
    [Authorize(Roles = "Refugio,Administrador")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoReporteDTO dto)
    {
        await _reporteService.CambiarEstadoAsync(id, dto, User.GetUserId(), User.IsInRole("Administrador"));
        return Ok(new { mensaje = "Estado del reporte actualizado." });
    }

    /// <summary>
    /// Convertir un reporte rescatado en una ficha de mascota del refugio autenticado.
    /// </summary>
    /// <response code="201">Mascota creada y vinculada al reporte.</response>
    /// <response code="400">Datos de mascota o catálogos inválidos.</response>
    /// <response code="403">El reporte no está asignado al refugio autenticado.</response>
    /// <response code="409">El reporte no está Rescatado o ya tiene una mascota vinculada.</response>
    [HttpPost("{id:int}/convertir-mascota", Name = "ConvertirReporteEnMascota")]
    [Authorize(Roles = "Refugio,Administrador")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ConvertirEnMascota(int id, [FromBody] ConvertirMascotaDTO dto)
    {
        var mascotaId = await _reporteService.ConvertirAFichaMascotaAsync(id, dto, User.GetUserId());

        return StatusCode(StatusCodes.Status201Created, new
        {
            mensaje = "Mascota creada desde el reporte.",
            mascotaId
        });
    }

    /// <summary>
    /// Reportes visibles en el mapa público.
    /// </summary>
    /// <remarks>
    /// Solo devuelve reportes activos en estados Recibido, EnAtencion o Rescatado,
    /// con coordenadas válidas y sin datos personales.
    /// </remarks>
    /// <response code="200">Lista de reportes para el mapa.</response>
    [HttpGet("mapa", Name = "GetMapaReportes")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<ReporteMapaDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMapa()
    {
        var result = await _reporteService.ObtenerReportesMapaAsync();
        return Ok(result);
    }
}
