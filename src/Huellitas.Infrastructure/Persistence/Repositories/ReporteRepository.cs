using System.Transactions;
using Huellitas.Infrastructure.Enums;
using Huellitas.Infrastructure.Interfaces.Repositories;
using Huellitas.Infrastructure.Persistence.Context;
using Huellitas.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Huellitas.Infrastructure.Persistence.Repositories;

public class ReporteRepository : IReporteRepository
{
    private readonly HuellitasDbContext _context;

    public ReporteRepository(HuellitasDbContext context) => _context = context;

    public async Task<ReportesCiudadano> InsertarAsync(
        ReportesCiudadano reporte,
        IEnumerable<ReporteMultimedia> fotos,
        CambiosDeEstado? cambioInicial)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // NumeroReporte es NOT NULL y no tiene índice único en la base de datos.
            // Se usa un valor temporal y luego se deriva del ReporteId para garantizar unicidad.
            reporte.NumeroReporte = GenerarNumeroTemporal();

            _context.ReportesCiudadanos.Add(reporte);
            await _context.SaveChangesAsync();

            reporte.NumeroReporte = GenerarNumeroDefinitivo(reporte.ReporteId);

            foreach (var foto in fotos)
            {
                foto.ReporteId = reporte.ReporteId;
                _context.ReporteMultimedia.Add(foto);
            }

            if (cambioInicial is not null)
            {
                cambioInicial.IdEntidad = reporte.ReporteId;
                _context.CambiosDeEstados.Add(cambioInicial);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return reporte;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<ReportesCiudadano?> ObtenerDetalleAsync(int reporteId)
    {
        return await _context.ReportesCiudadanos
            .AsNoTracking()
            .Include(r => r.Usuario)
            .Include(r => r.Refugio)
            .Include(r => r.Fotos)
            .FirstOrDefaultAsync(r => r.ReporteId == reporteId);
    }

    public async Task<ReportesCiudadano?> ObtenerParaConversionAsync(int reporteId)
    {
        return await _context.ReportesCiudadanos
            .AsNoTracking()
            .Include(r => r.Fotos)
            .FirstOrDefaultAsync(r => r.ReporteId == reporteId);
    }

    public async Task<ReportesCiudadano?> ObtenerBasicoAsync(int reporteId)
    {
        return await _context.ReportesCiudadanos
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.ReporteId == reporteId);
    }

    public async Task<(IEnumerable<ReportesCiudadano> Items, int Total)> ObtenerBandejaAsync(
        int refugioId,
        int pagina,
        int tamano)
    {
        // La bandeja incluye los reportes disponibles (Recibido y sin refugio)
        // y los que ya fueron asignados al refugio autenticado.
        var query = _context.ReportesCiudadanos
            .AsNoTracking()
            .Include(r => r.Usuario)
            .Include(r => r.Fotos)
            .Where(r => r.Activo &&
                        ((r.RefugioId == null && r.Estado == (byte)EstadoReporte.Recibido) ||
                         r.RefugioId == refugioId));

        var total = await query.CountAsync();

        var items = await query
            .OrderBy(r => r.RefugioId == null ? 0 : 1)
            .ThenBy(r => r.FechaReporte)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .ToListAsync();

        return (items, total);
    }

    public async Task<IEnumerable<ReportesCiudadano>> ObtenerMapaAsync()
    {
        var estadosVisibles = new[]
        {
            (byte)EstadoReporte.Recibido,
            (byte)EstadoReporte.EnAtencion,
            (byte)EstadoReporte.Rescatado
        };

        return await _context.ReportesCiudadanos
            .AsNoTracking()
            .Where(r => r.Activo &&
                        estadosVisibles.Contains(r.Estado) &&
                        r.Ubicacion_Latitud >= -90m && r.Ubicacion_Latitud <= 90m &&
                        r.Ubicacion_Longitud >= -180m && r.Ubicacion_Longitud <= 180m)
            .OrderByDescending(r => r.FechaReporte)
            .ToListAsync();
    }

    public async Task<bool> TomarAsync(int reporteId, int? refugioId, CambiosDeEstado cambio)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var filasAfectadas = await _context.ReportesCiudadanos
                .Where(r => r.ReporteId == reporteId &&
                            r.Activo &&
                            r.RefugioId == null &&
                            r.Estado == (byte)EstadoReporte.Recibido)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(r => r.RefugioId, (int?)refugioId)
                    .SetProperty(r => r.Estado, (byte)EstadoReporte.EnAtencion)
                    .SetProperty(r => r.FechaModificacion, (DateTime?)cambio.FechaCreacion));

            if (filasAfectadas == 0)
            {
                await transaction.RollbackAsync();
                return false;
            }

            _context.CambiosDeEstados.Add(cambio);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> CambiarEstadoAsync(
        int reporteId,
        int? refugioId,
        byte estadoAnterior,
        byte estadoNuevo,
        string? motivoCierre,
        CambiosDeEstado cambio)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var consulta = _context.ReportesCiudadanos
                .Where(r => r.ReporteId == reporteId &&
                            r.Activo &&
                            r.Estado == estadoAnterior);

            // Con refugio se exige pertenencia; sin refugio (administrador) no se filtra.
            if (refugioId.HasValue)
                consulta = consulta.Where(r => r.RefugioId == refugioId.Value);

            var filasAfectadas = await consulta
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(r => r.Estado, estadoNuevo)
                    .SetProperty(r => r.MotivoCierre, motivoCierre)
                    .SetProperty(r => r.FechaModificacion, (DateTime?)cambio.FechaCreacion));

            if (filasAfectadas == 0)
            {
                await transaction.RollbackAsync();
                return false;
            }

            _context.CambiosDeEstados.Add(cambio);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> VincularMascotaAsync(
        int reporteId,
        int refugioId,
        int mascotaId,
        CambiosDeEstado cambio)
    {
        // Si el llamador abrió una transacción ambiente (TransactionScope), se reutiliza
        // para que la creación de la mascota y el vínculo sean atómicos.
        var existeTransaccionAmbiente = Transaction.Current is not null;
        IDbContextTransaction? transaction = null;

        if (!existeTransaccionAmbiente)
            transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var filasAfectadas = await _context.ReportesCiudadanos
                .Where(r => r.ReporteId == reporteId &&
                            r.Activo &&
                            r.RefugioId == refugioId &&
                            r.MascotaId == null &&
                            r.Estado == (byte)EstadoReporte.Rescatado)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(r => r.MascotaId, (int?)mascotaId)
                    .SetProperty(r => r.FechaModificacion, (DateTime?)cambio.FechaCreacion));

            if (filasAfectadas == 0)
            {
                if (transaction is not null)
                    await transaction.RollbackAsync();

                return false;
            }

            _context.CambiosDeEstados.Add(cambio);
            await _context.SaveChangesAsync();

            if (transaction is not null)
                await transaction.CommitAsync();

            return true;
        }
        catch
        {
            if (transaction is not null)
                await transaction.RollbackAsync();

            throw;
        }
        finally
        {
            if (transaction is not null)
                await transaction.DisposeAsync();
        }
    }

    private static string GenerarNumeroTemporal()
    {
        // "TMP-" + 16 caracteres = 20, el máximo permitido por la columna.
        return $"TMP-{Guid.NewGuid():N}"[..20];
    }

    private static string GenerarNumeroDefinitivo(int reporteId)
    {
        return $"REP-{reporteId:D6}";
    }
}
