using Huellitas.API.Extensions;
using Huellitas.Application.DTOs.Refugios;
using Huellitas.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Huellitas.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class NecesidadesController : ControllerBase
{
    private readonly INecesidadService _necesidadService;

    public NecesidadesController(INecesidadService necesidadService) => _necesidadService = necesidadService;

    /// <summary>
    /// HU-REF-03: Publicar una necesidad de donación (Solo Refugio).
    /// </summary>
    [HttpPost(Name = "PublicarNecesidad")]
    [Authorize(Roles = "Refugio")]
    public async Task<IActionResult> Publicar([FromBody] CrearNecesidadDTO dto)
    {
        var id = await _necesidadService.PublicarNecesidadAsync(User.GetUserId(), dto);
        return Ok(new { mensaje = "Necesidad publicada exitosamente.", necesidadId = id });
    }

    /// <summary>
    /// HU-REF-03: Obtener las necesidades de un refugio (Público y paginado).
    /// </summary>
    [HttpGet("refugio/{refugioId}", Name = "GetNecesidadesRefugio")]
    [AllowAnonymous]
    public async Task<IActionResult> GetNecesidades(int refugioId, [FromQuery] int pagina = 1, [FromQuery] int tamano = 10)
    {
        var result = await _necesidadService.ObtenerNecesidadesRefugioAsync(refugioId, pagina, tamano);
        return Ok(result);
    }

    /// <summary>
    /// HU-REF-03: Marcar una necesidad como cubierta (Solo Refugio dueño).
    /// </summary>
    [HttpPatch("{id}/cubrir", Name = "MarcarNecesidadCubierta")]
    [Authorize(Roles = "Refugio")]
    public async Task<IActionResult> MarcarCubierta(int id)
    {
        await _necesidadService.MarcarComoCubiertaAsync(User.GetUserId(), id);
        return Ok(new { mensaje = "Necesidad marcada como cubierta." });
    }

    /// <summary>
    /// HU-REF-03: Editar una necesidad de donación (Solo Refugio dueño).
    /// </summary>
    [HttpPut("{id}", Name = "EditarNecesidad")]
    [Authorize(Roles = "Refugio")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Editar(int id, [FromBody] ActualizarNecesidadDTO dto)
    {
        await _necesidadService.EditarNecesidadAsync(User.GetUserId(), id, dto);
        return Ok(new { mensaje = "Necesidad actualizada correctamente." });
    }

    /// <summary>
    /// HU-REF-03: Eliminar una necesidad (Solo Refugio dueño).
    /// </summary>
    [HttpDelete("{id}", Name = "EliminarNecesidad")]
    [Authorize(Roles = "Refugio")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Eliminar(int id)
    {
        await _necesidadService.EliminarNecesidadAsync(User.GetUserId(), id);
        return Ok(new { mensaje = "Necesidad eliminada correctamente." });
    }
}