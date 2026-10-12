using Huellitas.Infrastructure.Interfaces.Repositories;
using Huellitas.Infrastructure.Persistence.Context;
using Huellitas.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Huellitas.Infrastructure.Persistence.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly HuellitasDbContext _context;

    public RefreshTokenRepository(HuellitasDbContext context) => _context = context;

    public async Task InsertarAsync(RefreshTokens token)
    {
        _context.RefreshTokens.Add(token);
        await _context.SaveChangesAsync();
    }

    public async Task ActualizarAsync(RefreshTokens token)
    {
        _context.RefreshTokens.Update(token);
        await _context.SaveChangesAsync();
    }

    public async Task<RefreshTokens?> ObtenerPorHashAsync(string hash)
    {
        return await _context.RefreshTokens
        .Include(r => r.Usuario)
        .FirstOrDefaultAsync(r => r.TokenHash == hash);
    }
        

    public async Task RevocarTodosDelUsuarioAsync(int usuarioId, string motivo)
    {
        var tokensActivos = await _context.RefreshTokens
            .Where(r => r.UsuarioId == usuarioId && !r.EstaRevocado)
            .ToListAsync();

        foreach (var token in tokensActivos)
        {
            token.EstaRevocado = true;
            token.RevocadoEn = DateTime.Now;
            token.ReemplazadoPor = motivo;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<int> ContarActivosDelUsuarioAsync(int usuarioId)
    {
        return await _context.RefreshTokens
        .CountAsync(r => r.UsuarioId == usuarioId && !r.EstaRevocado && r.Expira > DateTime.Now);
    }
       
}