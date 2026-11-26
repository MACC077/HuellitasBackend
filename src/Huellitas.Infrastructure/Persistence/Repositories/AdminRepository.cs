using Huellitas.Infrastructure.Enums;
using Huellitas.Infrastructure.Interfaces.Repositories;
using Huellitas.Infrastructure.Persistence.Context;
using Huellitas.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Huellitas.Infrastructure.Persistence.Repositories;

public class AdminRepository : IAdminRepository
{
    private readonly HuellitasDbContext _context;

    public AdminRepository(HuellitasDbContext context) => _context = context;

    // ADM-01: Usuarios

    public async Task<(IEnumerable<Usuarios> Items, int Total)> ObtenerUsuariosAsync(
        int pagina,
        int tamano,
        string? busqueda)
    {
        var query = _context.Usuarios
            .AsNoTracking()
            .Include(u => u.Rol)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var termino = busqueda.Trim();
            query = query.Where(u => u.Nombre.Contains(termino) || u.Correo.Contains(termino));
        }

        var total = await query.CountAsync();

        var items = await query
            .OrderBy(u => u.Nombre)
            .ThenBy(u => u.UsuarioId)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .ToListAsync();

        return (items, total);
    }

    public async Task<Usuarios?> ObtenerUsuarioConRolAsync(int usuarioId)
    {
        return await _context.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.UsuarioId == usuarioId);
    }

    public async Task<Roles?> ObtenerRolAsync(int rolId)
    {
        return await _context.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.RolId == rolId);
    }

    public async Task<int> ContarAdministradoresActivosAsync(int excluyendoUsuarioId)
    {
        return await _context.Usuarios.CountAsync(u =>
            u.UsuarioId != excluyendoUsuarioId &&
            u.RolId == (int)RolUsuario.Administrador &&
            u.Activo &&
            u.Estado == (byte)EstadoUsuario.Activo);
    }

    public async Task ActualizarUsuarioAsync(Usuarios usuario)
    {
        _context.Usuarios.Update(usuario);
        await _context.SaveChangesAsync();
    }

    // ADM-02: Denuncias

    public async Task<(IEnumerable<Denuncias> Items, int Total)> ObtenerDenunciasAsync(
        int pagina,
        int tamano,
        byte? estado)
    {
        var query = _context.Denuncias
            .AsNoTracking()
            .Include(d => d.Usuario)
            .Where(d => d.Activo)
            .AsQueryable();

        if (estado.HasValue)
            query = query.Where(d => d.Estado == estado.Value);

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(d => d.Fecha)
            .ThenByDescending(d => d.DenunciaId)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .ToListAsync();

        return (items, total);
    }

    public async Task<Denuncias?> ObtenerDenunciaAsync(int denunciaId)
    {
        return await _context.Denuncias
            .Include(d => d.Usuario)
            .FirstOrDefaultAsync(d => d.DenunciaId == denunciaId && d.Activo);
    }

    public async Task<bool> MarcarDenunciaResueltaAsync(int denunciaId, string resultado, DateTime fecha)
    {
        var filasAfectadas = await _context.Denuncias
            .Where(d => d.DenunciaId == denunciaId &&
                        d.Activo &&
                        d.Estado != (byte)EstadoDenuncia.Resuelta)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(d => d.Estado, (byte)EstadoDenuncia.Resuelta)
                .SetProperty(d => d.Resultado, resultado)
                .SetProperty(d => d.FechaModificacion, (DateTime?)fecha));

        return filasAfectadas > 0;
    }

    public async Task<bool> ExisteContenidoAsync(byte tipoContenido, int idContenido)
    {
        return tipoContenido switch
        {
            (byte)TipoContenidoDenuncia.Mascota =>
                await _context.Mascotas.AnyAsync(m => m.MascotaId == idContenido),
            (byte)TipoContenidoDenuncia.Refugio =>
                await _context.Refugios.AnyAsync(r => r.RefugioId == idContenido),
            (byte)TipoContenidoDenuncia.Calificacion =>
                await _context.Calificaciones.AnyAsync(c => c.CalificacionId == idContenido),
            (byte)TipoContenidoDenuncia.Reporte =>
                await _context.ReportesCiudadanos.AnyAsync(r => r.ReporteId == idContenido),
            _ => false
        };
    }

    public async Task<bool> OcultarContenidoAsync(byte tipoContenido, int idContenido, DateTime fecha)
    {
        var filasAfectadas = tipoContenido switch
        {
            (byte)TipoContenidoDenuncia.Mascota => await _context.Mascotas
                .Where(m => m.MascotaId == idContenido && m.Activo)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(m => m.Activo, false)
                    .SetProperty(m => m.Estado, (byte)EstadoMascota.NoDisponible)
                    .SetProperty(m => m.MotivoBaja, "Ocultada por resolucion de denuncia.")
                    .SetProperty(m => m.FechaModificacion, (DateTime?)fecha)
                    .SetProperty(m => m.FechaUltimaActualizacion, fecha)),

            (byte)TipoContenidoDenuncia.Refugio => await _context.Refugios
                .Where(r => r.RefugioId == idContenido && r.Activo)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(r => r.Activo, false)
                    .SetProperty(r => r.FechaModificacion, (DateTime?)fecha)),

            (byte)TipoContenidoDenuncia.Calificacion => await _context.Calificaciones
                .Where(c => c.CalificacionId == idContenido && c.Activo)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(c => c.Activo, false)
                    .SetProperty(c => c.FechaModificacion, (DateTime?)fecha)),

            (byte)TipoContenidoDenuncia.Reporte => await _context.ReportesCiudadanos
                .Where(r => r.ReporteId == idContenido && r.Activo)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(r => r.Activo, false)
                    .SetProperty(r => r.FechaModificacion, (DateTime?)fecha)),

            _ => 0
        };

        return filasAfectadas > 0;
    }

    // ADM-03: Configuración

    public async Task<IEnumerable<ConfiguracionSistema>> ObtenerConfiguracionesAsync()
    {
        return await _context.ConfiguracionSistemas
            .AsNoTracking()
            .OrderBy(c => c.Clave)
            .ToListAsync();
    }

    public async Task<ConfiguracionSistema?> ObtenerConfiguracionAsync(int configuracionId)
    {
        return await _context.ConfiguracionSistemas
            .FirstOrDefaultAsync(c => c.ConfiguracionId == configuracionId);
    }

    public async Task<bool> ExisteClaveConfiguracionAsync(string clave, int? excluyendoId)
    {
        var query = _context.ConfiguracionSistemas.Where(c => c.Clave == clave);

        if (excluyendoId.HasValue)
            query = query.Where(c => c.ConfiguracionId != excluyendoId.Value);

        return await query.AnyAsync();
    }

    public async Task InsertarConfiguracionAsync(ConfiguracionSistema configuracion)
    {
        _context.ConfiguracionSistemas.Add(configuracion);
        await _context.SaveChangesAsync();
    }

    public async Task ActualizarConfiguracionAsync(ConfiguracionSistema configuracion)
    {
        _context.ConfiguracionSistemas.Update(configuracion);
        await _context.SaveChangesAsync();
    }

    // ADM-04: Dashboard

    public async Task<int> ContarAdopcionesExitosasAsync()
    {
        return await _context.Adopciones.CountAsync(a =>
            a.Activo &&
            a.Solicitud.Activo &&
            a.Solicitud.Estado == (byte)EstadoSolicitudAdopcion.Finalizada);
    }

    public async Task<int> ContarMascotasDisponiblesAsync()
    {
        return await _context.Mascotas.CountAsync(m =>
            m.Activo &&
            m.Estado == (byte)EstadoMascota.Disponible);
    }

    public async Task<int> ContarUsuariosActivosAsync()
    {
        return await _context.Usuarios.CountAsync(u => u.Activo);
    }

    public async Task<int> ContarRefugiosVerificadosAsync()
    {
        return await _context.Refugios.CountAsync(r =>
            r.Activo &&
            r.EstadoVerificacion == (byte)EstadoVerificacionRefugio.Verificado);
    }

    public async Task<int> ContarReportesCiudadanosAsync()
    {
        return await _context.ReportesCiudadanos.CountAsync(r => r.Activo);
    }

    public async Task<double> ObtenerTiempoPromedioAdopcionDiasAsync()
    {
        var promedio = await _context.Adopciones
            .Where(a => a.Activo &&
                        a.Solicitud.Activo &&
                        a.Solicitud.Estado == (byte)EstadoSolicitudAdopcion.Finalizada)
            .AverageAsync(a => (double?)EF.Functions.DateDiffDay(a.Solicitud.FechaEnvio, a.FechaFinalizacion));

        return promedio ?? 0d;
    }
}
