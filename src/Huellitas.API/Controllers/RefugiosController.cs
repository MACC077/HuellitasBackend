using Huellitas.API.Extensions;
using Huellitas.Application.DTOs.Refugios;
using Huellitas.Application.Interfaces.Services;
using Huellitas.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Huellitas.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class RefugiosController : ControllerBase
{
    private readonly IRefugioService _refugioService;
    private readonly ICalificacionService _calificacionService;
    public RefugiosController(IRefugioService refugioService, ICalificacionService calificacionService)
    {
        _refugioService = refugioService;
        _calificacionService = calificacionService;
    }

    /// <summary>HU-REF-02: Obtener el perfil del refugio autenticado.</summary>
    [HttpGet("mi-perfil", Name = "GetPerfilRefugio")]
    [Authorize(Roles = "Refugio")]
    public async Task<IActionResult> GetPerfil()
    {
        var result = await _refugioService.ObtenerMiPerfilAsync(User.GetUserId());
        return Ok(result);
    }

    /// <summary>HU-REF-02: Actualizar los datos del perfil del refugio.</summary>
    [HttpPut("mi-perfil", Name = "UpdatePerfilRefugio")]
    [Authorize(Roles = "Refugio")]
    public async Task<IActionResult> UpdatePerfil([FromBody] ActualizarRefugioDTO dto)
    {
        await _refugioService.ActualizarPerfilAsync(User.GetUserId(), dto);
        return Ok(new { mensaje = "Perfil actualizado correctamente." });
    }

    /// <summary>HU-REF-02: Agregar un requisito de adopción.</summary>
    [HttpPost("requisitos", Name = "AgregarRequisito")]
    [Authorize(Roles = "Refugio")]
    public async Task<IActionResult> AgregarRequisito([FromBody] RequisitoDTO dto)
    {
        await _refugioService.AgregarRequisitoAsync(User.GetUserId(), dto);
        return Ok(new { mensaje = "Requisito agregado." });
    }

    /// <summary>
    /// HU-REF-01: Solicitar el registro de un nuevo refugio (Cualquier usuario autenticado).
    /// </summary>
    /// <remarks>
    /// El refugio quedará en estado Pendiente (0) hasta que un Administrador lo verifique.
    /// El usuario que lo crea automáticamente pasa a tener el rol de Refugio.
    /// </remarks>
    [HttpPost("solicitar", Name = "SolicitarRegistroRefugio")]
    [Authorize] // Cualquier usuario logueado puede solicitar ser refugio
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Solicitar([FromBody] CrearRefugioDTO dto)
    {
        await _refugioService.SolicitarRegistroRefugioAsync(User.GetUserId(), dto);
        return Ok(new { mensaje = "Solicitud de registro enviada. Un administrador revisará tu caso." });
    }

    /// <summary>HU-REF-01: Verificar o rechazar un refugio (Solo Administrador).</summary>
    [HttpPatch("{id:int}/verificar", Name = "VerificarRefugio")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Verificar(int id, [FromBody] VerificarRefugioDTO dto)
    {
        await _refugioService.VerificarRefugioAsync(id, dto, User.GetUserId());
        return Ok(new { mensaje = dto.Aprobado ? "Refugio verificado correctamente." : "Refugio rechazado." });
    }

    /// <summary>ADM-01: Suspender un refugio (Solo Administrador).</summary>
    [HttpPatch("{id:int}/suspender", Name = "SuspenderRefugio")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Suspender(int id, [FromBody] SuspenderRefugioDTO dto)
    {
        await _refugioService.SuspenderRefugioAsync(id, dto, User.GetUserId());
        return Ok(new { mensaje = "Refugio suspendido." });
    }

    /// <summary>ADM-01: Listar refugios pendientes de verificación (Solo Administrador).</summary>
    [HttpGet("pendientes", Name = "GetRefugiosPendientes")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPendientes([FromQuery] int pagina = 1, [FromQuery] int tamano = 10)
    {
        var result = await _refugioService.ObtenerRefugiosPendientesAsync(pagina, tamano);
        return Ok(result);
    }

    /// <summary>ADM-01: Consultar los soportes cargados por un refugio (Solo Administrador).</summary>
    [HttpGet("{id:int}/soportes", Name = "GetSoportesRefugio")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSoportes(int id)
    {
        var result = await _refugioService.ObtenerSoportesAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// HU-REF-02: Listar los refugios verificados (Público).
    /// </summary>
    [HttpGet(Name = "GetRefugiosPublicos")]
    [AllowAnonymous] //Sin token
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRefugios([FromQuery] int pagina = 1, [FromQuery] int tamano = 10)
    {
        var result = await _refugioService.ObtenerRefugiosPublicosAsync(pagina, tamano);
        return Ok(result);
    }

    /// <summary>
    /// HU-REF-02: Obtener el perfil público de un refugio (Público).
    /// </summary>
    [HttpGet("{id:int}", Name = "GetPerfilPublicoRefugio")]
    [AllowAnonymous] //Sin token
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRefugioPublico(int id)
    {
        var result = await _refugioService.ObtenerPerfilPublicoAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// HU-REF-04: Calificar a un refugio después de finalizar una adopción.
    /// </summary>
    [HttpPost("{id}/calificaciones", Name = "CalificarRefugio")]
    [Authorize(Roles = "Adoptante")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Calificar(int id, [FromBody] CrearCalificacionDTO dto)
    {
        await _calificacionService.CalificarRefugioAsync(User.GetUserId(), id, dto);
        return Ok(new { mensaje = "¡Gracias por tu calificación!" });
    }

    /// <summary>
    /// HU-REF-04: Obtener las calificaciones públicas de un refugio (Paginado).
    /// </summary>
    [HttpGet("{id}/calificaciones", Name = "GetCalificacionesRefugio")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCalificaciones(int id, [FromQuery] int pagina = 1, [FromQuery] int tamano = 10)
    {
        var result = await _calificacionService.ObtenerCalificacionesRefugioAsync(id, User.GetUserId(), pagina, tamano);
        return Ok(result);
    }

    /// <summary>
    /// HU-REF-04: Editar una calificación existente (Solo el autor de la calificación).
    /// </summary>
    [HttpPut("{id}/calificaciones/{calificacionId}", Name = "EditarCalificacion")]
    [Authorize(Roles = "Adoptante")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EditarCalificacion(int id, int calificacionId, [FromBody] ActualizarCalificacionDTO dto)
    {
        await _calificacionService.EditarCalificacionAsync(User.GetUserId(), id, calificacionId, dto);
        return Ok(new { mensaje = "Calificación actualizada correctamente." });
    }

    /// <summary>
    /// HU-REF-04: Eliminar una calificación (Solo el autor de la calificación).
    /// </summary>
    [HttpDelete("{id}/calificaciones/{calificacionId}", Name = "EliminarCalificacion")]
    [Authorize(Roles = "Adoptante")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EliminarCalificacion(int id, int calificacionId)
    {
        await _calificacionService.EliminarCalificacionAsync(User.GetUserId(), id, calificacionId);
        return Ok(new { mensaje = "Calificación eliminada correctamente." });
    }
}