using Huellitas.API.Extensions;
using Huellitas.Application.Common;
using Huellitas.Application.DTOs.Administracion;
using Huellitas.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Huellitas.API.Controllers;

/// <summary>
/// Módulo de Administración (ADM-01 a ADM-04).
/// Todas las operaciones requieren el rol Administrador.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Roles = "Administrador")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService) => _adminService = adminService;

    // ─── ADM-01: Usuarios y refugios ────────────────────────────────

    /// <summary>
    /// Consultar usuarios con búsqueda por nombre o correo y paginación.
    /// </summary>
    /// <response code="200">Lista paginada de usuarios.</response>
    /// <response code="400">Paginación inválida.</response>
    [HttpGet("usuarios", Name = "GetUsuariosAdmin")]
    [ProducesResponseType(typeof(PagedResult<UsuarioAdminDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetUsuarios(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamano = 10,
        [FromQuery] string? busqueda = null)
    {
        var result = await _adminService.ObtenerUsuariosAsync(pagina, tamano, busqueda);
        return Ok(result);
    }

    /// <summary>
    /// Activar o desactivar una cuenta de usuario.
    /// </summary>
    /// <remarks>
    /// El motivo es obligatorio al desactivar. Un administrador no puede desactivar
    /// su propia cuenta ni dejar el sistema sin administradores activos.
    /// </remarks>
    /// <response code="200">Estado actualizado o sin cambios.</response>
    /// <response code="400">Datos inválidos o intento de auto-desactivación.</response>
    /// <response code="404">El usuario no existe.</response>
    /// <response code="409">Se intenta desactivar al último administrador activo.</response>
    [HttpPatch("usuarios/{id:int}/estado", Name = "CambiarEstadoUsuarioAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoUsuarioDTO dto)
    {
        var cambio = await _adminService.CambiarEstadoUsuarioAsync(id, dto, User.GetUserId());

        return Ok(new
        {
            mensaje = cambio
                ? "Estado del usuario actualizado."
                : "El usuario ya tenía el estado indicado."
        });
    }

    /// <summary>
    /// Cambiar el rol de un usuario.
    /// </summary>
    /// <remarks>
    /// Al dejar de ser Refugio se elimina el vínculo con el refugio. No se puede
    /// retirar el rol al último administrador activo.
    /// </remarks>
    /// <response code="200">Rol actualizado o sin cambios.</response>
    /// <response code="400">Rol inexistente o inactivo.</response>
    /// <response code="404">El usuario no existe.</response>
    /// <response code="409">Se intenta retirar el rol al último administrador activo.</response>
    [HttpPatch("usuarios/{id:int}/rol", Name = "CambiarRolUsuarioAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CambiarRol(int id, [FromBody] CambiarRolUsuarioDTO dto)
    {
        var cambio = await _adminService.CambiarRolUsuarioAsync(id, dto, User.GetUserId());

        return Ok(new
        {
            mensaje = cambio
                ? "Rol del usuario actualizado."
                : "El usuario ya tenía el rol indicado."
        });
    }

    // ─── ADM-02: Denuncias ──────────────────────────────────────────

    /// <summary>
    /// Consultar denuncias activas con filtro opcional por estado.
    /// </summary>
    /// <param name="estado">0 = Recibida, 1 = EnRevision, 2 = Resuelta.</param>
    /// <response code="200">Lista paginada de denuncias.</response>
    /// <response code="400">Paginación o estado inválido.</response>
    [HttpGet("denuncias", Name = "GetDenunciasAdmin")]
    [ProducesResponseType(typeof(PagedResult<DenunciaDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetDenuncias(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamano = 10,
        [FromQuery] byte? estado = null)
    {
        var result = await _adminService.ObtenerDenunciasAsync(pagina, tamano, estado);
        return Ok(result);
    }

    /// <summary>
    /// Resolver una denuncia y, opcionalmente, ocultar el contenido denunciado.
    /// </summary>
    /// <remarks>
    /// Solo se pueden resolver denuncias en estado Recibida o EnRevision. Al ocultar:
    /// una mascota pasa a NoDisponible, y refugios, calificaciones y reportes se
    /// marcan como inactivos.
    /// </remarks>
    /// <response code="200">Denuncia resuelta.</response>
    /// <response code="400">Resultado faltante o tipo de contenido inválido.</response>
    /// <response code="404">La denuncia o el contenido denunciado no existe.</response>
    /// <response code="409">La denuncia ya fue resuelta o no es resoluble.</response>
    [HttpPatch("denuncias/{id:int}/resolver", Name = "ResolverDenunciaAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ResolverDenuncia(int id, [FromBody] ResolverDenunciaDTO dto)
    {
        await _adminService.ResolverDenunciaAsync(id, dto, User.GetUserId());
        return Ok(new { mensaje = "Denuncia resuelta." });
    }

    // ─── ADM-03: Configuración ──────────────────────────────────────

    /// <summary>
    /// Consultar la configuración del sistema.
    /// </summary>
    /// <remarks>
    /// Los valores de claves sensibles se devuelven enmascarados como ***.
    /// </remarks>
    /// <response code="200">Lista de configuraciones.</response>
    [HttpGet("configuracion", Name = "GetConfiguracionAdmin")]
    [ProducesResponseType(typeof(IEnumerable<ConfiguracionDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetConfiguracion()
    {
        var result = await _adminService.ObtenerConfiguracionesAsync();
        return Ok(result);
    }

    /// <summary>
    /// Crear un parámetro de configuración.
    /// </summary>
    /// <response code="201">Configuración creada.</response>
    /// <response code="400">Datos inválidos o clave sensible.</response>
    /// <response code="409">Ya existe una configuración con esa clave.</response>
    [HttpPost("configuracion", Name = "CrearConfiguracionAdmin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CrearConfiguracion([FromBody] CrearConfiguracionDTO dto)
    {
        var id = await _adminService.CrearConfiguracionAsync(dto, User.GetUserId());

        return StatusCode(StatusCodes.Status201Created, new
        {
            mensaje = "Configuración creada.",
            configuracionId = id
        });
    }

    /// <summary>
    /// Actualizar un parámetro de configuración existente.
    /// </summary>
    /// <response code="200">Configuración actualizada.</response>
    /// <response code="400">Datos inválidos o clave sensible.</response>
    /// <response code="404">La configuración no existe.</response>
    /// <response code="409">Ya existe otra configuración con esa clave.</response>
    [HttpPut("configuracion/{id:int}", Name = "ActualizarConfiguracionAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ActualizarConfiguracion(int id, [FromBody] CrearConfiguracionDTO dto)
    {
        await _adminService.ActualizarConfiguracionAsync(id, dto, User.GetUserId());
        return Ok(new { mensaje = "Configuración actualizada." });
    }

    // ─── ADM-04: Dashboard ──────────────────────────────────────────

    /// <summary>
    /// Métricas generales del sistema para el panel administrativo.
    /// </summary>
    /// <remarks>
    /// Criterios aplicados: adopciones exitosas = adopción activa cuya solicitud está
    /// activa y en estado Finalizada; mascotas disponibles = activas y en estado
    /// Disponible; usuarios = cuentas con Activo = true; refugios verificados =
    /// activos y en estado Verificado; reportes = reportes ciudadanos activos.
    /// El tiempo promedio se calcula en días entre FechaEnvio y FechaFinalizacion y
    /// se redondea a un decimal.
    /// </remarks>
    /// <response code="200">Métricas calculadas.</response>
    [HttpGet("dashboard", Name = "GetDashboardAdmin")]
    [ProducesResponseType(typeof(DashboardMetricasDTO), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard()
    {
        var result = await _adminService.ObtenerMetricasAsync();
        return Ok(result);
    }
}
