using Huellitas.Infrastructure.Interfaces.Repositories;
using Huellitas.Infrastructure.Persistence.Context;
using Huellitas.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Huellitas.Infrastructure.Persistence.Repositories;

public class RefugioRepository : IRefugioRepository
{
    private readonly HuellitasDbContext _context;
    public RefugioRepository(HuellitasDbContext context) => _context = context;

    public async Task<Refugios?> ObtenerPorUsuarioIdAsync(int usuarioId)
    {
        // El scaffold de EF Core genera la propiedad de navegación 'Refugio' en Usuarios
        // gracias a la FK RefugioId.
        var usuario = await _context.Usuarios
            .Include(u => u.Refugio)
            .FirstOrDefaultAsync(u => u.UsuarioId == usuarioId);

        return usuario?.Refugio;
    }
}