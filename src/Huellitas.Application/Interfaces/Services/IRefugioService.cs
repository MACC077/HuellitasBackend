using Huellitas.Application.Common;
using Huellitas.Application.DTOs.Refugios;

namespace Huellitas.Application.Interfaces.Services;

public interface IRefugioService
{
    Task<PerfilRefugioDTO> ObtenerMiPerfilAsync(int usuarioId);
    Task ActualizarPerfilAsync(int usuarioId, ActualizarRefugioDTO dto);
    Task AgregarRequisitoAsync(int usuarioId, RequisitoDTO dto);
    Task EliminarRequisitoAsync(int usuarioId, int requisitoId);
    Task VerificarRefugioAsync(int refugioId, VerificarRefugioDTO dto);
    Task<PagedResult<RefugioResumenDTO>> ObtenerRefugiosPublicosAsync(int pagina, int tamano);
    Task<RefugioPublicoDTO> ObtenerPerfilPublicoAsync(int refugioId);
}