using Huellitas.Application.DTOs.Catalogos;
using Huellitas.Application.Interfaces.Services;
using Huellitas.Application.Mappings;
using Huellitas.Infrastructure.Interfaces.Repositories;

namespace Huellitas.Application.Services;

public class CatalogoService : ICatalogoService
{
    private readonly ICatalogoRepository _repo;

    public CatalogoService(ICatalogoRepository repo) => _repo = repo;

    public async Task<IEnumerable<EspecieDTO>> ObtenerEspeciesAsync()
    {
        var especies = await _repo.ObtenerEspeciesAsync();
        return especies.ToDtoList();
    }

    public async Task<IEnumerable<RazaDTO>> ObtenerRazasAsync(int especieId)
    {
        var razas = await _repo.ObtenerRazasPorEspecieAsync(especieId);
        return razas.ToDtoList();
    }

    public async Task<IEnumerable<TamanoDTO>> ObtenerTamanosAsync()
    {
        var tamanos = await _repo.ObtenerTamanosAsync();
        return tamanos.ToDtoList();
    }

    public async Task<IEnumerable<ColorDTO>> ObtenerColoresAsync()
    {
        var colores = await _repo.ObtenerColoresAsync();
        return colores.ToDtoList();
    }

    public async Task<IEnumerable<TemperamentoDTO>> ObtenerTemperamentosAsync()
    {
        var temperamentos = await _repo.ObtenerTemperamentosAsync();
        return temperamentos.ToDtoList();
    }
}