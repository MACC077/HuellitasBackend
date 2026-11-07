using Huellitas.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
}