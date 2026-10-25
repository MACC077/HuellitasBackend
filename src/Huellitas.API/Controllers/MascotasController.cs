using Huellitas.API.Extensions;
using Huellitas.Application.DTOs.Mascotas;
using Huellitas.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Huellitas.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Roles = "Refugio,Administrador")]
public class MascotasController : ControllerBase
{
    private readonly IMascotaService _mascotaService;

    public MascotasController(IMascotaService mascotaService) => _mascotaService = mascotaService;

    /// <summary>
    /// Registrar una nueva mascota (Solo Refugios verificados).
    /// </summary>
    /// <response code="201">Mascota registrada exitosamente.</response>
    /// <response code="400">Datos inválidos o el usuario no es un refugio.</response>
    /// <response code="403">El refugio no está verificado.</response>
    [HttpPost(Name = "RegistrarMascota")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Registrar([FromBody] CrearMascotaDTO dto)
    {
        var id = await _mascotaService.RegistrarAsync(dto, User.GetUserId());
        return StatusCode(StatusCodes.Status201Created, new { mensaje = "Mascota registrada.", id });
    }

    /// <summary>
    /// Actualizar los datos de una mascota existente.
    /// </summary>
    /// <param name="id">ID de la mascota a actualizar.</param>
    /// <response code="200">Mascota actualizada correctamente.</response>
    /// <response code="403">El usuario no es dueño de la mascota.</response>
    /// <response code="404">Mascota no encontrada.</response>
    [HttpPut("{id}", Name = "ActualizarMascota")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarMascotaDTO dto)
    {
        await _mascotaService.ActualizarAsync(id, dto, User.GetUserId());
        return Ok(new { mensaje = "Mascota actualizada." });
    }

    /// <summary>
    /// Cambiar el estado de una mascota (Disponible, En Proceso, Adoptada, No Disponible).
    /// </summary>
    /// <param name="id">ID de la mascota.</param>
    /// <response code="200">Estado actualizado correctamente.</response>
    /// <response code="400">Motivo faltante al intentar pasar a "No Disponible".</response>
    [HttpPatch("{id}/estado", Name = "CambiarEstadoMascota")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoMascotaDTO dto)
    {
        await _mascotaService.CambiarEstadoAsync(id, dto, User.GetUserId());
        return Ok(new { mensaje = "Estado actualizado." });
    }

    /// <summary>
    /// Dar de baja lógicamente una mascota del inventario del refugio.
    /// </summary>
    /// <param name="id">ID de la mascota.</param>
    /// <param name="motivo">Razón por la cual se da de baja.</param>
    /// <response code="200">Mascota dada de baja.</response>
    /// <response code="409">No se puede dar de baja porque tiene solicitudes activas.</response>
    [HttpDelete("{id}", Name = "DarDeBajaMascota")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DarDeBaja(int id, [FromQuery] string motivo)
    {
        await _mascotaService.DarDeBajaAsync(id, motivo, User.GetUserId());
        return Ok(new { mensaje = "Mascota dada de baja." });
    }

    /// <summary>
    /// Obtener el inventario de mascotas del refugio autenticado.
    /// </summary>
    /// <remarks>
    /// Este endpoint solo devuelve las mascotas que pertenecen al refugio 
    /// cuyo usuario está logueado en el token actual.
    /// </remarks>
    /// <response code="200">Lista paginada de mascotas.</response>
    [HttpGet("refugio/inventario", Name = "GetInventarioRefugio")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInventarioRefugio([FromQuery] int pagina = 1, [FromQuery] int tamano = 10)
    {
        // Nota: Mantengo el nombre del método en el servicio como ObtenerMisMascotasAsync
        // porque a nivel de negocio, para el refugio, sí son "sus" mascotas.
        var result = await _mascotaService.ObtenerMisMascotasAsync(User.GetUserId(), pagina, tamano);
        return Ok(result);
    }
}