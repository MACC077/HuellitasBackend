using Huellitas.API.Extensions;
using Huellitas.Application.DTOs.Denuncias;
using Huellitas.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Huellitas.API.Controllers;

/// <summary>
/// Registro de denuncias sobre fichas, perfiles, reportes y comentarios.
/// Cualquier usuario autenticado puede denunciar; la bandeja de gestión es administrativa.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class DenunciasController : ControllerBase
{
    private readonly IDenunciaService _denunciaService;

    public DenunciasController(IDenunciaService denunciaService) => _denunciaService = denunciaService;

    /// <summary>
    /// Registrar una denuncia sobre contenido visible de la plataforma.
    /// </summary>
    /// <remarks>
    /// TipoContenido: 0 = Mascota, 1 = Refugio (perfil), 2 = Calificación (comentario), 3 = Reporte.
    /// Motivo: 0 = PosibleEstafa, 1 = InformacionFalsa, 2 = ContenidoInapropiado, 3 = Otro.
    /// </remarks>
    /// <response code="201">Denuncia registrada.</response>
    /// <response code="400">Datos inválidos.</response>
    /// <response code="404">El contenido denunciado no existe o no está disponible.</response>
    /// <response code="409">Ya existe una denuncia pendiente del mismo usuario sobre el contenido.</response>
    [HttpPost(Name = "CrearDenuncia")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Crear([FromBody] CrearDenunciaDTO dto)
    {
        var id = await _denunciaService.CrearAsync(User.GetUserId(), dto);

        return StatusCode(StatusCodes.Status201Created, new
        {
            mensaje = "Denuncia registrada. El equipo administrativo la revisará.",
            denunciaId = id
        });
    }
}
