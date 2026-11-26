using Huellitas.Application.Common;
using Huellitas.Application.DTOs.Refugios;

namespace Huellitas.Application.Interfaces.Services;

public interface IApadrinamientoService
{
    Task<int> SolicitarApadrinamientoAsync(int usuarioId, CrearApadrinamientoDTO dto);
    Task GestionarApadrinamientoAsync(int usuarioId, int apadrinamientoId, GestionarApadrinamientoDTO dto);
    Task<PagedResult<ApadrinamientoDTO>> ObtenerMisApadrinamientosAsync(int usuarioId, int pagina, int tamano);
    Task<PagedResult<ApadrinamientoDTO>> ObtenerApadrinamientosRefugioAsync(int usuarioId, int pagina, int tamano);
    Task CancelarApadrinamientoAsync(int usuarioId, int apadrinamientoId);
}