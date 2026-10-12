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
}