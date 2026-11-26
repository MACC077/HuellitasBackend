using Huellitas.Application.Common;
using Huellitas.Application.Common.Exceptions;
using Huellitas.Application.DTOs.Refugios;
using Huellitas.Application.Interfaces.Services;
using Huellitas.Application.Mappings;
using Huellitas.Infrastructure.Enums;
using Huellitas.Infrastructure.Interfaces.Repositories;
using Huellitas.Infrastructure.Persistence.Entities;

namespace Huellitas.Application.Services;

public class ApadrinamientoService : IApadrinamientoService
{
    private readonly IApadrinamientoRepository _repo;
    private readonly IMascotaRepository _mascotaRepo;
    private readonly IRefugioRepository _refugioRepo;

    public ApadrinamientoService(IApadrinamientoRepository repo, IMascotaRepository mascotaRepo, IRefugioRepository refugioRepo)
    {
        _repo = repo;
        _mascotaRepo = mascotaRepo;
        _refugioRepo = refugioRepo;
    }

    public async Task<int> SolicitarApadrinamientoAsync(int usuarioId, CrearApadrinamientoDTO dto)
    {
        var mascota = await _mascotaRepo.ObtenerPorIdAsync(dto.MascotaId)
            ?? throw AppException.NotFound("Mascota no encontrada.");

        if (!mascota.Activo)
            throw AppException.BadRequest("La mascota no está activa.");

        var apadrinamiento = new Apadrinamientos
        {
            UsuarioId = usuarioId,
            MascotaId = dto.MascotaId,
            FechaInicio = DateOnly.FromDateTime(DateTime.Now),
            Estado = (byte)EstadoApadrinamiento.Solicitado,
            Observacion = dto.Observacion,
            Activo = true,
            FechaCreacion = DateTime.Now
        };

        await _repo.InsertarAsync(apadrinamiento);
        return apadrinamiento.ApadrinamientoId;
    }

    public async Task GestionarApadrinamientoAsync(int usuarioId, int apadrinamientoId, GestionarApadrinamientoDTO dto)
    {
        var refugio = await _refugioRepo.ObtenerPorUsuarioIdAsync(usuarioId)
            ?? throw AppException.BadRequest("Usuario no asociado a un refugio.");

        var apadrinamiento = await _repo.ObtenerPorIdAsync(apadrinamientoId)
            ?? throw AppException.NotFound("Solicitud de apadrinamiento no encontrada.");

        if (apadrinamiento.Mascota.RefugioId != refugio.RefugioId)
            throw AppException.Forbidden("No tienes permiso para gestionar este apadrinamiento.");

        if (apadrinamiento.Estado != (byte)EstadoApadrinamiento.Solicitado)
            throw AppException.Conflict("Solo se pueden gestionar solicitudes en estado 'Solicitado'.");

        if (dto.Aceptar)
        {
            apadrinamiento.Estado = (byte)EstadoApadrinamiento.Activo;
            apadrinamiento.Observacion = dto.Observacion ?? "Solicitud aceptada.";
        }
        else
        {
            apadrinamiento.Estado = (byte)EstadoApadrinamiento.Finalizado; // O podrías usar inactivo
            apadrinamiento.Observacion = dto.Observacion ?? "Solicitud rechazada.";
            apadrinamiento.Activo = false; // Se rechaza, así que lo desactivamos
        }

        apadrinamiento.FechaModificacion = DateTime.Now;
        await _repo.ActualizarAsync(apadrinamiento);
    }

    public async Task<PagedResult<ApadrinamientoDTO>> ObtenerMisApadrinamientosAsync(int usuarioId, int pagina, int tamano)
    {
        var (items, total) = await _repo.ObtenerPorUsuarioAsync(usuarioId, pagina, tamano);

        return new PagedResult<ApadrinamientoDTO>
        {
            Items = items.Select(a => a.ToDto()).ToList(),
            Pagina = pagina,
            TamanoPagina = tamano,
            TotalRegistros = total
        };
    }

    public async Task<PagedResult<ApadrinamientoDTO>> ObtenerApadrinamientosRefugioAsync(int usuarioId, int pagina, int tamano)
    {
        var refugio = await _refugioRepo.ObtenerPorUsuarioIdAsync(usuarioId)
            ?? throw AppException.BadRequest("Usuario no asociado a un refugio.");

        var (items, total) = await _repo.ObtenerPorRefugioAsync(refugio.RefugioId, pagina, tamano);

        return new PagedResult<ApadrinamientoDTO>
        {
            Items = items.Select(a => a.ToDto()).ToList(),
            Pagina = pagina,
            TamanoPagina = tamano,
            TotalRegistros = total
        };
    }

    public async Task CancelarApadrinamientoAsync(int usuarioId, int apadrinamientoId)
    {
        var apadrinamiento = await _repo.ObtenerPorIdAsync(apadrinamientoId)
            ?? throw AppException.NotFound("Apadrinamiento no encontrado o ya se encuentra inactivo.");

        // 1. Validar que el usuario es el dueño del apadrinamiento
        if (apadrinamiento.UsuarioId != usuarioId)
            throw AppException.Forbidden("No tienes permiso para cancelar este apadrinamiento.");

        // 2. Cancelar (sin importar si estaba Solicitado o Activo)
        apadrinamiento.Estado = (byte)EstadoApadrinamiento.Finalizado;
        apadrinamiento.Observacion = "Cancelado voluntariamente por el padrino.";
        apadrinamiento.Activo = false;
        apadrinamiento.FechaModificacion = DateTime.Now;

        await _repo.ActualizarAsync(apadrinamiento);
    }
}