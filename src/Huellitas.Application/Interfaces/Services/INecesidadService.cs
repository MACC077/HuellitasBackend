using Huellitas.Application.Common;
using Huellitas.Application.DTOs.Refugios;

namespace Huellitas.Application.Interfaces.Services;

public interface INecesidadService
{
    Task<int> PublicarNecesidadAsync(int usuarioId, CrearNecesidadDTO dto);
    Task<PagedResult<NecesidadDTO>> ObtenerNecesidadesRefugioAsync(int refugioId, int pagina, int tamano);
    Task EditarNecesidadAsync(int usuarioId, int necesidadId, ActualizarNecesidadDTO dto);
    Task EliminarNecesidadAsync(int usuarioId, int necesidadId);
    Task MarcarComoCubiertaAsync(int usuarioId, int necesidadId);
}