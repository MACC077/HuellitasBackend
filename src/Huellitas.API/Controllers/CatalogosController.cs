using Asp.Versioning;
using Huellitas.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Huellitas.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class CatalogosController : ControllerBase
{
    private readonly ICatalogoService _catalogoService;

    public CatalogosController(ICatalogoService catalogoService)
    {
        _catalogoService = catalogoService;
    }

    [HttpGet("especies", Name = "GetEspecies")]
    public async Task<IActionResult> GetEspecies()
    {
        var result = await _catalogoService.ObtenerEspeciesAsync();
        return Ok(result);
    }

    [HttpGet("razas", Name = "GetRazas")]
    public async Task<IActionResult> GetRazas([FromQuery] int especieId)
    {
        // Si no envían especieId, retornamos lista vacía o un 400
        if (especieId <= 0)
            return Ok(Enumerable.Empty<object>());

        var result = await _catalogoService.ObtenerRazasAsync(especieId);
        return Ok(result);
    }

    [HttpGet("tamanos", Name = "GetTamanos")]
    public async Task<IActionResult> GetTamanos()
    {
        var result = await _catalogoService.ObtenerTamanosAsync();
        return Ok(result);
    }

    [HttpGet("colores", Name = "GetColores")]
    public async Task<IActionResult> GetColores()
    {
        var result = await _catalogoService.ObtenerColoresAsync();
        return Ok(result);
    }

    [HttpGet("temperamentos", Name = "GetTemperamentos")]
    public async Task<IActionResult> GetTemperamentos()
    {
        var result = await _catalogoService.ObtenerTemperamentosAsync();
        return Ok(result);
    }
}