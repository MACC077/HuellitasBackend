using Huellitas.Application.DTOs.Catalogos;

namespace Huellitas.Application.Interfaces.Services;

public interface ICatalogoService
{
    Task<IEnumerable<EspecieDTO>> ObtenerEspeciesAsync();
    Task<IEnumerable<RazaDTO>> ObtenerRazasAsync(int especieId);
    Task<IEnumerable<TamanoDTO>> ObtenerTamanosAsync();
    Task<IEnumerable<ColorDTO>> ObtenerColoresAsync();
    Task<IEnumerable<TemperamentoDTO>> ObtenerTemperamentosAsync();
}