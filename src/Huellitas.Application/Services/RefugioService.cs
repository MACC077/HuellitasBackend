using Huellitas.Application.Common;
using Huellitas.Application.Common.Exceptions;
using Huellitas.Application.DTOs.Refugios;
using Huellitas.Application.Interfaces.Services;
using Huellitas.Application.Mappings;
using Huellitas.Infrastructure.Interfaces.Repositories;

namespace Huellitas.Application.Services;

public class RefugioService : IRefugioService
{
    private readonly IRefugioRepository _repo;
    public RefugioService(IRefugioRepository repo) => _repo = repo;

    public async Task<PerfilRefugioDTO> ObtenerMiPerfilAsync(int usuarioId)
    {
        var refugio = await _repo.ObtenerPorUsuarioIdAsync(usuarioId)
            ?? throw AppException.NotFound("No tienes un refugio asociado.");
        return refugio.ToPerfilDto();
    }

    public async Task ActualizarPerfilAsync(int usuarioId, ActualizarRefugioDTO dto)
    {
        var refugio = await _repo.ObtenerPorUsuarioIdAsync(usuarioId)
            ?? throw AppException.NotFound("No tienes un refugio asociado.");

        refugio.Descripcion = dto.Descripcion;
        refugio.Direccion = dto.Direccion;
        refugio.Telefono = dto.Telefono;
        refugio.RedesSociales = dto.RedesSociales;
        refugio.FechaModificacion = DateTime.Now;

        await _repo.ActualizarAsync(refugio);
    }

    public async Task AgregarRequisitoAsync(int usuarioId, RequisitoDTO dto)
    {
        var refugio = await _repo.ObtenerPorUsuarioIdAsync(usuarioId)
            ?? throw AppException.NotFound("No tienes un refugio asociado.");

        var requisito = dto.ToEntity(refugio.RefugioId);
        await _repo.InsertarRequisitoAsync(requisito);
    }

    public async Task EliminarRequisitoAsync(int usuarioId, int requisitoId)
    {
        var refugio = await _repo.ObtenerPorUsuarioIdAsync(usuarioId)
            ?? throw AppException.NotFound("No tienes un refugio asociado.");

        // Aquí idealmente validarías que el requisito pertenece al refugio antes de borrar
        await _repo.EliminarRequisitoAsync(requisitoId);
    }

    public async Task VerificarRefugioAsync(int refugioId, VerificarRefugioDTO dto)
    {
        var refugio = await _repo.ObtenerPorIdAsync(refugioId)
            ?? throw AppException.NotFound("Refugio no encontrado.");

        if (dto.Aprobado)
        {
            refugio.EstadoVerificacion = 1; // Verificado
            refugio.FechaVerificacion = DateTime.Now;
            refugio.MotivoRechazo = null;
        }
        else
        {
            if (string.IsNullOrWhiteSpace(dto.MotivoRechazo))
                throw AppException.BadRequest("El motivo de rechazo es obligatorio.");

            refugio.EstadoVerificacion = 2; // Rechazado
            refugio.MotivoRechazo = dto.MotivoRechazo;
        }

        await _repo.ActualizarAsync(refugio);
    }

    public async Task<PagedResult<RefugioResumenDTO>> ObtenerRefugiosPublicosAsync(int pagina, int tamano)
    {
        var (items, total) = await _repo.ObtenerRefugiosPublicosAsync(pagina, tamano);

        return new PagedResult<RefugioResumenDTO>
        {
            Items = items.Select(r => r.ToResumenDto()).ToList(),
            Pagina = pagina,
            TamanoPagina = tamano,
            TotalRegistros = total
        };
    }

    public async Task<RefugioPublicoDTO> ObtenerPerfilPublicoAsync(int refugioId)
    {
        var refugio = await _repo.ObtenerPerfilPublicoPorIdAsync(refugioId)
            ?? throw AppException.NotFound("Refugio no encontrado o no está verificado.");

        return refugio.ToPublicoDto();
    }
}