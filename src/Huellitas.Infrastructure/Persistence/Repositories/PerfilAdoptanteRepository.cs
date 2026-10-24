using Huellitas.Infrastructure.Interfaces.Repositories;
using Huellitas.Infrastructure.Persistence.Context;
using Huellitas.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Huellitas.Infrastructure.Persistence.Repositories;

public class PerfilAdoptanteRepository : IPerfilAdoptanteRepository
{
    private readonly HuellitasDbContext _context;
    public PerfilAdoptanteRepository(HuellitasDbContext context) => _context = context;

    public async Task<PerfilesAdoptante?> ObtenerPorUsuarioIdAsync(int usuarioId)
    {
        return await _context.PerfilesAdoptantes.FirstOrDefaultAsync(p => p.UsuarioId == usuarioId);
    }

    public async Task<PerfilesAdoptante> InsertarAsync(PerfilesAdoptante perfil)
    {
        _context.PerfilesAdoptantes.Add(perfil);
        await _context.SaveChangesAsync();
        return perfil;
    }

    public async Task ActualizarAsync(PerfilesAdoptante perfil)
    {
        _context.PerfilesAdoptantes.Update(perfil);
        await _context.SaveChangesAsync();
    }
}