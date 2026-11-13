using Huellitas.API.Extensions;
using Huellitas.Application.DTOs.Refugios;
using Huellitas.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Huellitas.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class RefugiosController : ControllerBase
{
    private readonly IRefugioService _refugioService;
    public RefugiosController(IRefugioService refugioService) => _refugioService = refugioService;

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

    /// <summary>HU-REF-01: Verificar o rechazar un refugio (Solo Administrador).</summary>
    [HttpPatch("{id}/verificar", Name = "VerificarRefugio")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Verificar(int id, [FromBody] VerificarRefugioDTO dto)
    {
        await _refugioService.VerificarRefugioAsync(id, dto);
        return Ok(new { mensaje = dto.Aprobado ? "Refugio verificado correctamente." : "Refugio rechazado." });
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
    [HttpGet("{id}", Name = "GetPerfilPublicoRefugio")]
    [AllowAnonymous] //Sin token
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRefugioPublico(int id)
    {
        var result = await _refugioService.ObtenerPerfilPublicoAsync(id);
        return Ok(result);
    }
}