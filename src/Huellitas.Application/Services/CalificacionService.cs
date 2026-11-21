using Huellitas.Application.Common;
using Huellitas.Application.Common.Exceptions;
using Huellitas.Application.DTOs.Refugios;
using Huellitas.Application.Interfaces.Services;
using Huellitas.Infrastructure.Interfaces.Repositories;
using Huellitas.Infrastructure.Persistence.Entities;

namespace Huellitas.Application.Services;

public class CalificacionService : ICalificacionService
{
    private readonly ICalificacionRepository _calificacionRepo;
    private readonly ISolicitudAdopcionRepository _solicitudRepo;

    public CalificacionService(ICalificacionRepository calificacionRepo, ISolicitudAdopcionRepository solicitudRepo)
    {
        _calificacionRepo = calificacionRepo;
        _solicitudRepo = solicitudRepo;
    }

    public async Task CalificarRefugioAsync(int usuarioId, int refugioId, CrearCalificacionDTO dto)
    {
        //Validar que el puntaje esté en el rango
        if (dto.Puntaje < 1 || dto.Puntaje > 5)
            throw AppException.BadRequest("El puntaje debe ser entre 1 y 5.");

        //Validar que la adopción existe y está finalizada
        var adopcion = await _solicitudRepo.ObtenerAdopcionConDetallesAsync(dto.AdopcionId);
        if (adopcion == null)
            throw AppException.NotFound("Adopción no encontrada.");

        //Validar que el usuario que califica sea el que adoptó
        if (adopcion.Solicitud.UsuarioId != usuarioId)
            throw AppException.Forbidden("No tienes permiso para calificar esta adopción.");

        //Validar que la adopción pertenezca al refugio
        if (adopcion.Solicitud.Mascota.RefugioId != refugioId)
            throw AppException.BadRequest("La adopción no corresponde a este refugio.");

        //Validar que no haya calificado antes
        if (await _calificacionRepo.ExisteCalificacionParaAdopcionAsync(dto.AdopcionId))
            throw AppException.Conflict("Ya has calificado esta adopción.");

        //Guardar la calificación
        var calificacion = new Calificaciones
        {
            RefugioId = refugioId,
            UsuarioId = usuarioId,
            AdopcionId = dto.AdopcionId,
            Puntaje = dto.Puntaje,
            Comentario = dto.Comentario,
            EstadoModeracion = 0, // 0 = Publicada
            Fecha = DateTime.Now,
            Activo = true
        };

        await _calificacionRepo.InsertarAsync(calificacion);
    }

    public async Task<PagedResult<CalificacionDTO>> ObtenerCalificacionesRefugioAsync(int refugioId, int usuarioId, int pagina, int tamano)
    {
        var (items, total) = await _calificacionRepo.ObtenerPorRefugioAsync(refugioId, pagina, tamano);

        return new PagedResult<CalificacionDTO>
        {
            Items = items.Select(c => new CalificacionDTO
            {
                CalificacionId = c.CalificacionId,
                AdoptanteNombre = c.Usuario?.Nombre ?? "Adoptante Anónimo",
                Puntaje = c.Puntaje,
                Comentario = c.Comentario,
                Fecha = c.Fecha,
                EsPropia = (usuarioId > 0 && c.UsuarioId == usuarioId)
            }).ToList(),
            Pagina = pagina,
            TamanoPagina = tamano,
            TotalRegistros = total
        };
    }

    public async Task EditarCalificacionAsync(int usuarioId, int refugioId, int calificacionId, ActualizarCalificacionDTO dto)
    {
        var calificacion = await _calificacionRepo.ObtenerPorIdAsync(calificacionId)
            ?? throw AppException.NotFound("Calificación no encontrada.");

        // Validar que la calificación pertenece al usuario y al refugio
        if (calificacion.UsuarioId != usuarioId || calificacion.RefugioId != refugioId)
            throw AppException.Forbidden("No tienes permiso para editar esta calificación.");

        if (dto.Puntaje < 1 || dto.Puntaje > 5)
            throw AppException.BadRequest("El puntaje debe ser entre 1 y 5.");

        calificacion.Puntaje = dto.Puntaje;
        calificacion.Comentario = dto.Comentario;
        calificacion.FechaModificacion = DateTime.Now; 

        await _calificacionRepo.ActualizarAsync(calificacion);
    }

    public async Task EliminarCalificacionAsync(int usuarioId, int refugioId, int calificacionId)
    {
        var calificacion = await _calificacionRepo.ObtenerPorIdAsync(calificacionId)
            ?? throw AppException.NotFound("Calificación no encontrada.");

        // Validar que la calificación pertenece al usuario y al refugio
        if (calificacion.UsuarioId != usuarioId || calificacion.RefugioId != refugioId)
            throw AppException.Forbidden("No tienes permiso para eliminar esta calificación.");

        // Borrado lógico (Soft Delete)
        calificacion.Activo = false;
        await _calificacionRepo.ActualizarAsync(calificacion);
    }
}