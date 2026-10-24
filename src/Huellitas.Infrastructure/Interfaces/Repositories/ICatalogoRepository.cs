using Huellitas.Infrastructure.Persistence.Entities;

namespace Huellitas.Infrastructure.Interfaces.Repositories;

public interface ICatalogoRepository
{
    Task<IEnumerable<Especies>> ObtenerEspeciesAsync();
    Task<IEnumerable<Razas>> ObtenerRazasPorEspecieAsync(int especieId);
    Task<IEnumerable<Tamanos>> ObtenerTamanosAsync();
    Task<IEnumerable<Colores>> ObtenerColoresAsync();
    Task<IEnumerable<Temperamentos>> ObtenerTemperamentosAsync();
    Task<IEnumerable<TiposVivienda>> ObtenerTiposViviendaAsync();
}