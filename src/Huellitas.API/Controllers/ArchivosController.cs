using Huellitas.API.Extensions;
using Huellitas.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Huellitas.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize] //Cualquier usuario logueado puede subir archivos
public class ArchivosController : ControllerBase
{
    private readonly IArchivoService _archivoService;

    public ArchivosController(IArchivoService archivoService) => _archivoService = archivoService;

    /// <summary>
    /// Sube un archivo a la nube (Cloudinary) y devuelve su URL pública.
    /// </summary>
    /// <param name="archivo">El archivo binario a subir.</param>
    /// <param name="tipo">Carpeta destino: perfiles, mascotas, soportes, refugios o reportes.</param>
    [HttpPost(Name = "SubirArchivo")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Subir(IFormFile archivo, [FromForm] string tipo)
    {
        if (archivo == null || archivo.Length == 0)
            return BadRequest(new { mensaje = "Debes adjuntar un archivo válido." });

        using var stream = archivo.OpenReadStream();
        var result = await _archivoService.SubirArchivoAsync(stream, archivo.FileName, tipo);

        return Ok(new { mensaje = "Archivo subido exitosamente.", url = result.Url });
    }

    /// <summary>
    /// Subir un soporte de refugio (PDF o imagen) sin requerir sesión.
    /// </summary>
    /// <remarks>
    /// Se usa durante el registro público de refugios. Solo acepta PDF o imágenes
    /// (jpg, jpeg, png, webp) de hasta 5 MB y se almacena en la carpeta "soportes".
    /// </remarks>
    /// <param name="archivo">Archivo PDF o imagen.</param>
    /// <response code="200">Soporte subido y URL devuelta.</response>
    /// <response code="400">Archivo inválido, extensión no permitida o tamaño excedido.</response>
    /// <response code="429">Demasiadas solicitudes desde esta IP.</response>
    [HttpPost("soporte-publico", Name = "SubirSoportePublico")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingExtensions.PoliticaReportes)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> SubirSoportePublico(IFormFile archivo)
    {
        if (archivo == null || archivo.Length == 0)
            return BadRequest(new { mensaje = "Debes adjuntar un archivo válido." });

        using var stream = archivo.OpenReadStream();
        var result = await _archivoService.SubirArchivoAsync(stream, archivo.FileName, "soportes");

        return Ok(new { mensaje = "Soporte subido exitosamente.", url = result.Url });
    }
}