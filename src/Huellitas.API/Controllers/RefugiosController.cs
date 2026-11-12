using Huellitas.API.Extensions;
using Huellitas.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Huellitas.API.Controllers;

[ApiController]
[Route("api/refugios")]
[Route("api/v{version:apiVersion}/[controller]")]
public class RefugiosController(HuellitasDbContext context) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Listar([FromQuery] int pagina = 1, [FromQuery] int tamanoPagina = 10)
    {
        if (pagina < 1 || tamanoPagina < 1 || tamanoPagina > 100)
            return BadRequest(new { mensaje = "La página debe ser positiva y el tamaño debe estar entre 1 y 100." });
        var consulta = context.Refugios.AsNoTracking().Where(r => r.Activo);
        var total = await consulta.CountAsync();
        var items = await consulta.OrderBy(r => r.RefugioId)
            .Skip((pagina - 1) * tamanoPagina).Take(tamanoPagina)
            .Select(r => new { id = r.RefugioId, r.Nombre, r.Nit, r.Direccion, r.Telefono,
                r.Correo, r.Encargado, r.RedesSociales, r.Descripcion, r.EstadoVerificacion,
                r.FechaVerificacion, r.MotivoRechazo, r.Activo, r.FechaCreacion, r.FechaModificacion })
            .ToListAsync();
        return Ok(new { items, pagina, tamanoPagina, total, totalPaginas = (int)Math.Ceiling((double)total / tamanoPagina) });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Obtener(int id)
    {
        var usuarioId = User.GetUserId();
        var administrador = User.IsInRole("Administrador");
        var refugio = await context.Refugios.AsNoTracking()
            .Where(r => r.RefugioId == id && r.Activo &&
                (r.EstadoVerificacion == 1 || administrador || r.Usuarios.Any(u => u.UsuarioId == usuarioId && u.Activo)))
            .Select(r => new { id = r.RefugioId, r.Nombre, r.Nit, r.Direccion, r.Telefono,
                r.Correo, r.Encargado, r.RedesSociales, r.Descripcion, r.EstadoVerificacion,
                r.FechaVerificacion, r.MotivoRechazo, r.Activo, r.FechaCreacion, r.FechaModificacion,
                promedioCalificacion = r.Calificaciones.Where(c => c.Activo && c.EstadoModeracion == 0).Average(c => (double?)c.Puntaje),
                totalAdopciones = context.Adopciones.Count(a => a.Activo && a.Solicitud.Mascota.RefugioId == r.RefugioId),
                requisitos = r.RequisitosAdopcions.Where(q => q.Activo).OrderBy(q => q.Orden)
                    .Select(q => new { id = q.RequisitoId, q.RefugioId, q.Descripcion, q.EsObligatorio,
                        q.Orden, q.Activo, q.FechaCreacion, q.FechaModificacion }).ToList() })
            .SingleOrDefaultAsync();
        return refugio is null ? NotFound() : Ok(refugio);
    }
}
