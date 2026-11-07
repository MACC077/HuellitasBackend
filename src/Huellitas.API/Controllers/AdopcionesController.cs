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

    /// <summary>
    /// HU-ADO-04: Finalizar la adopción y cerrar las demás solicitudes automáticamente.
    /// </summary>
    [HttpPatch("{id}/finalizar", Name = "FinalizarAdopcion")]
    [Authorize(Roles = "Refugio,Administrador")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Finalizar(int id)
    {
        await _adopcionService.FinalizarAdopcionAsync(id, User.GetUserId());
        return Ok(new { mensaje = "Adopción finalizada exitosamente. Se ha generado la constancia." });
    }

    /// <summary>
    /// HU-ADO-05: Agendar una entrevista o visita (Solo Refugio).
    /// </summary>
    [HttpPost("{id}/agendar-visita", Name = "AgendarVisita")]
    [Authorize(Roles = "Refugio,Administrador")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AgendarVisita(int id, [FromBody] AgendarVisitaDTO dto)
    {
        await _adopcionService.AgendarVisitaAsync(id, dto, User.GetUserId());
        return Ok(new { mensaje = "Visita agendada. El adoptante ha sido notificado." });
    }

    /// <summary>
    /// HU-ADO-05: Confirmar o rechazar la fecha de la visita (Solo Adoptante).
    /// </summary>
    [HttpPatch("{id}/confirmar-visita", Name = "ConfirmarVisita")]
    [Authorize(Roles = "Adoptante")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ConfirmarVisita(int id, [FromBody] ConfirmarVisitaDTO dto)
    {
        await _adopcionService.ConfirmarVisitaAsync(id, dto, User.GetUserId());
        return Ok(new { mensaje = dto.Aceptar ? "Visita confirmada." : "Fecha rechazada. El refugio propondrá una nueva." });
    }

    // ═══════════════════════════════════════════════════════
    // HU-ADO-06: Soportes
    // ═══════════════════════════════════════════════════════

    /// <summary>
    /// HU-ADO-06: Asociar un documento (previamente subido a Cloudinary) a la solicitud.
    /// </summary>
    /// <remarks>
    /// El archivo físico debe subirse primero a POST /api/v1/Archivos con tipo="soportes".
    /// </remarks>
    [HttpPost("{id}/soportes", Name = "GuardarSoporte")]
    [Authorize(Roles = "Adoptante")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GuardarSoporte(int id, [FromBody] CrearDocumentoSoporteDTO dto)
    {
        //Recibe el JSON con Tipo y ArchivoUrl (ya no es IFormFile)
        var result = await _adopcionService.GuardarDocumentoAsync(id, dto, User.GetUserId());
        return Ok(new { mensaje = "Documento registrado exitosamente.", documento = result });
    }

    /// <summary>
    /// HU-ADO-06: Listar los documentos de soporte de una solicitud.
    /// </summary>
    [HttpGet("{id}/soportes", Name = "GetSoportes")]
    [Authorize(Roles = "Adoptante,Refugio,Administrador")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSoportes(int id)
    {
        var result = await _adopcionService.ObtenerDocumentosAsync(id, User.GetUserId());
        return Ok(result);
    }

    /// <summary>
    /// HU-ADO-07: Descargar la constancia de adopción en PDF.
    /// </summary>
    [HttpGet("{id}/constancia", Name = "DescargarConstancia")]
    [Authorize(Roles = "Adoptante,Refugio,Administrador")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DescargarConstancia(int id)
    {
        var pdfBytes = await _adopcionService.GenerarConstanciaPdfAsync(id, User.GetUserId());
        //Retorna el archivo al navegador para que se descargue
        return File(pdfBytes, "application/pdf", $"Constancia_Adopcion_Huellitas_{id}.pdf");
    }
}