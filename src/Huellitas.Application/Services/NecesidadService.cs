using Huellitas.Application.Common;
using Huellitas.Application.Common.Exceptions;
using Huellitas.Application.DTOs.Refugios;
using Huellitas.Application.Interfaces.Services;
using Huellitas.Infrastructure.Enums;
using Huellitas.Infrastructure.Interfaces.Repositories;
using Huellitas.Infrastructure.Persistence.Entities;

namespace Huellitas.Application.Services;

public class NecesidadService : INecesidadService
{
    private readonly INecesidadRepository _repo;
    private readonly IRefugioRepository _refugioRepo;

    public NecesidadService(INecesidadRepository repo, IRefugioRepository refugioRepo)
    {
        _repo = repo;
        _refugioRepo = refugioRepo;
    }

    public async Task<int> PublicarNecesidadAsync(int usuarioId, CrearNecesidadDTO dto)
    {
        var refugio = await _refugioRepo.ObtenerPorUsuarioIdAsync(usuarioId)
            ?? throw AppException.BadRequest("Usuario no asociado a un refugio.");

        var necesidad = new Necesidades
        {
            RefugioId = refugio.RefugioId,
            Tipo = dto.Tipo,
            Descripcion = dto.Descripcion,
            MedioDonacion = dto.MedioDonacion,
            Estado = (byte)EstadoNecesidad.Publicada,
            FechaPublicacion = DateTime.Now,
            Activo = true,
            FechaCreacion = DateTime.Now
        };

        await _repo.InsertarAsync(necesidad);
        return necesidad.NecesidadId;
    }

    public async Task<PagedResult<NecesidadDTO>> ObtenerNecesidadesRefugioAsync(int refugioId, int pagina, int tamano)
    {
        var (items, total) = await _repo.ObtenerPorRefugioAsync(refugioId, pagina, tamano);

        return new PagedResult<NecesidadDTO>
        {
            Items = items.Select(n => new NecesidadDTO
            {
                NecesidadId = n.NecesidadId,
                Tipo = ((TipoNecesidad)n.Tipo).ToString(),
                Descripcion = n.Descripcion,
                MedioDonacion = n.MedioDonacion,
                Estado = ((EstadoNecesidad)n.Estado).ToString(),
                FechaPublicacion = n.FechaPublicacion
            }).ToList(),
            Pagina = pagina,
            TamanoPagina = tamano,
            TotalRegistros = total
        };
    }

    public async Task EditarNecesidadAsync(int usuarioId, int necesidadId, ActualizarNecesidadDTO dto)
    {
        var refugio = await _refugioRepo.ObtenerPorUsuarioIdAsync(usuarioId)
            ?? throw AppException.BadRequest("Usuario no asociado a un refugio.");

        var necesidad = await _repo.ObtenerPorIdAsync(necesidadId)
            ?? throw AppException.NotFound("Necesidad no encontrada.");

        if (necesidad.RefugioId != refugio.RefugioId)
            throw AppException.Forbidden("No tienes permiso para editar esta necesidad.");

        necesidad.Tipo = dto.Tipo;
        necesidad.Descripcion = dto.Descripcion;
        necesidad.MedioDonacion = dto.MedioDonacion;
        necesidad.FechaModificacion = DateTime.Now;

        await _repo.ActualizarAsync(necesidad);
    }

    public async Task EliminarNecesidadAsync(int usuarioId, int necesidadId)
    {
        var refugio = await _refugioRepo.ObtenerPorUsuarioIdAsync(usuarioId)
            ?? throw AppException.BadRequest("Usuario no asociado a un refugio.");

        var necesidad = await _repo.ObtenerPorIdAsync(necesidadId)
            ?? throw AppException.NotFound("Necesidad no encontrada.");

        if (necesidad.RefugioId != refugio.RefugioId)
            throw AppException.Forbidden("No tienes permiso para eliminar esta necesidad.");

        // Borrado lógico
        necesidad.Activo = false;
        necesidad.FechaModificacion = DateTime.Now;

        await _repo.ActualizarAsync(necesidad);
    }

    public async Task MarcarComoCubiertaAsync(int usuarioId, int necesidadId)
    {
        var refugio = await _refugioRepo.ObtenerPorUsuarioIdAsync(usuarioId)
            ?? throw AppException.BadRequest("Usuario no asociado a un refugio.");

        var necesidad = await _repo.ObtenerPorIdAsync(necesidadId)
            ?? throw AppException.NotFound("Necesidad no encontrada.");

        if (necesidad.RefugioId != refugio.RefugioId)
            throw AppException.Forbidden("No tienes permiso para modificar esta necesidad.");

        necesidad.Estado = (byte)EstadoNecesidad.Cubierta;
        necesidad.FechaModificacion = DateTime.Now;

        await _repo.ActualizarAsync(necesidad);
    }
}