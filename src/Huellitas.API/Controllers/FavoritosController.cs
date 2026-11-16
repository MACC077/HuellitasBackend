using Huellitas.API.Extensions;
using Huellitas.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Huellitas.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Roles = "Adoptante")] // Solo los adoptantes guardan favoritos
public class FavoritosController : ControllerBase
{
    private readonly IFavoritoService _favoritoService;
    public FavoritosController(IFavoritoService favoritoService) => _favoritoService = favoritoService;

    /// <summary>
    /// HU-MAS-07: Listar las mascotas favoritas del adoptante (Paginado).
    /// </summary>
    [HttpGet(Name = "GetMisFavoritos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMisFavoritos([FromQuery] int pagina = 1, [FromQuery] int tamano = 10)
    {
        var result = await _favoritoService.ObtenerMisFavoritosAsync(User.GetUserId(), pagina, tamano);
        return Ok(result);
    }

    /// <summary>
    /// HU-MAS-07: Agregar una mascota a favoritos.
    /// </summary>
    [HttpPost("{mascotaId}", Name = "AgregarFavorito")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Agregar(int mascotaId)
    {
        await _favoritoService.AgregarFavoritoAsync(User.GetUserId(), mascotaId);
        return Ok(new { mensaje = "Mascota agregada a favoritos." });
    }

    /// <summary>
    /// HU-MAS-07: Eliminar una mascota de favoritos.
    /// </summary>
    [HttpDelete("{mascotaId}", Name = "EliminarFavorito")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Eliminar(int mascotaId)
    {
        await _favoritoService.EliminarFavoritoAsync(User.GetUserId(), mascotaId);
        return Ok(new { mensaje = "Mascota eliminada de favoritos." });
    }
}