using Huellitas.Infrastructure.Enums;
using Huellitas.Infrastructure.Interfaces.Repositories;
using Huellitas.Infrastructure.Persistence.Context;
using Huellitas.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Huellitas.Infrastructure.Persistence.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly HuellitasDbContext _context;

    public UsuarioRepository(HuellitasDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExisteCorreoAsync(string correo) => await _context.Usuarios.AnyAsync(u => u.Correo == correo);

    public async Task<Usuarios> InsertarAsync(Usuarios usuario)
    {
        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();
        return usuario;
    }

    public async Task<Usuarios?> ObtenerPorCorreoAsync(string correo)
    {
        return await _context.Usuarios
        .Include(u => u.Rol)
        .FirstOrDefaultAsync(u => u.Correo == correo);
    }
        
    public async Task<Usuarios?> ObtenerPorIdAsync(int id)
    {
        return await _context.Usuarios
        .Include(u => u.Rol)
        .FirstOrDefaultAsync(u => u.UsuarioId == id);
    }

    public async Task ActualizarAsync(Usuarios usuario)
    {
        _context.Usuarios.Update(usuario);
        await _context.SaveChangesAsync();
    }

    public async Task<int> ObtenerRolIdPorNombreAsync(string nombreRol)
    {
        var rol = await _context.Roles.FirstOrDefaultAsync(r => r.Nombre == nombreRol);
        return rol?.RolId ?? 0;
    }

    public async Task<int> ObtenerPoliticaActivaIdAsync()
    {
        var politica = await _context.PoliticasDatos.FirstOrDefaultAsync(p => p.Activo);
        return politica?.PoliticaId ?? 0;
    }

    public async Task<PoliticasDato?> ObtenerPoliticaActivaAsync()
    {
        return await _context.PoliticasDatos
            .AsNoTracking()
            .Where(p => p.Activo)
            .OrderByDescending(p => p.FechaPublicacion)
            .FirstOrDefaultAsync();
    }

    public async Task<PoliticasDato?> ObtenerPoliticaPorIdAsync(int politicaId)
    {
        return await _context.PoliticasDatos
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PoliticaId == politicaId);
    }

    public async Task<IEnumerable<PoliticasDato>> ObtenerPoliticasAsync()
    {
        return await _context.PoliticasDatos
            .AsNoTracking()
            .OrderByDescending(p => p.FechaPublicacion)
            .ThenByDescending(p => p.PoliticaId)
            .ToListAsync();
    }

    public async Task InsertarPoliticaAsync(PoliticasDato politica)
    {
        _context.PoliticasDatos.Add(politica);
        await _context.SaveChangesAsync();
    }

    public async Task DesactivarPoliticasActivasAsync()
    {
        await _context.PoliticasDatos
            .Where(p => p.Activo)
            .ExecuteUpdateAsync(setters => setters.SetProperty(p => p.Activo, false));
    }

    public async Task<bool> HaAceptadoPoliticaAsync(int usuarioId, int politicaId)
    {
        return await _context.ConsentimientosDatos.AnyAsync(c =>
            c.UsuarioId == usuarioId &&
            c.PoliticaId == politicaId &&
            c.Activo);
    }

    public async Task DesactivarConsentimientosActivosAsync(int usuarioId)
    {
        await _context.ConsentimientosDatos
            .Where(c => c.UsuarioId == usuarioId && c.Activo)
            .ExecuteUpdateAsync(setters => setters.SetProperty(c => c.Activo, false));
    }

    public async Task InsertarTokenAsync(TokensSeguridad token)
    {
        _context.TokensSeguridad.Add(token);
        await _context.SaveChangesAsync();
    }

    public async Task InsertarConsentimientoAsync(ConsentimientosDatos consentimiento)
    {
        _context.ConsentimientosDatos.Add(consentimiento);
        await _context.SaveChangesAsync();
    }

    public async Task<TokensSeguridad?> ObtenerTokenPorValorAsync(string valor)
    {
        return await _context.TokensSeguridad.FirstOrDefaultAsync(t => t.Valor == valor);
    }

    public async Task ActualizarTokenAsync(TokensSeguridad token)
    {
       _context.TokensSeguridad.Update(token);
        await _context.SaveChangesAsync();
    }

    public async Task<TokensSeguridad?> ObtenerTokenActivoPorUsuarioAsync(int usuarioId, TipoToken tipo)
    {
        var token =  await _context.TokensSeguridad
        .Where(t => t.UsuarioId == usuarioId 
               && t.Tipo == (byte)tipo 
               && t.Activo 
               && !t.Usado 
               && t.FechaExpiracion > DateTime.UtcNow)
            .OrderByDescending(t => t.FechaCreacion)
            .FirstOrDefaultAsync();
        return token;
    }

    public async Task ActualizarContrasenaAsync(int usuarioId, string nuevoHash)
    {
        var usuario = await _context.Usuarios.FindAsync(usuarioId);
        if (usuario != null)
        {
            usuario.ContrasenaHash = nuevoHash;
            usuario.FechaModificacion = DateTime.Now;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Usuarios?> ObtenerPerfilCompletoAsync(int usuarioId)
    {
        return await _context.Usuarios
            .Include(u => u.ConsentimientosDatos)
            .ThenInclude(c => c.Politica)
            .FirstOrDefaultAsync(u => u.UsuarioId == usuarioId);
    }

    public async Task<Usuarios?> ObtenerPorRefugioIdAsync(int refugioId)
    {
        return await _context.Usuarios
            .FirstOrDefaultAsync(u => u.RefugioId == refugioId && u.Activo);
    }
}