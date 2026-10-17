using Huellitas.Infrastructure.Interfaces.Repositories;
using Huellitas.Infrastructure.Persistence.Context;
using Huellitas.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Huellitas.Infrastructure.Persistence.Repositories;

public class CatalogoRepository : ICatalogoRepository
{
    private readonly HuellitasDbContext _context;

    public CatalogoRepository(HuellitasDbContext context) => _context = context;

    public async Task<IEnumerable<Especies>> ObtenerEspeciesAsync()
    {
        return await _context.Especies.Where(e => e.Activo).OrderBy(e => e.Nombre).ToListAsync();
    }
       
    public async Task<IEnumerable<Razas>> ObtenerRazasPorEspecieAsync(int especieId)
    {
        return await _context.Razas.Where(r => r.EspecieId == especieId && r.Activo).OrderBy(r => r.Nombre).ToListAsync();
    }

    public async Task<IEnumerable<Tamanos>> ObtenerTamanosAsync()
    {
        return await _context.Tamanos.Where(t => t.Activo).OrderBy(t => t.Nombre).ToListAsync();    
    }

    public async Task<IEnumerable<Colores>> ObtenerColoresAsync()
    {
        return await _context.Colores.Where(c => c.Activo).OrderBy(c => c.Nombre).ToListAsync();
    }
       
    public async Task<IEnumerable<Temperamentos>> ObtenerTemperamentosAsync()
    {
        return await _context.Temperamentos.Where(t => t.Activo).OrderBy(t => t.Nombre).ToListAsync();
    }
}
       