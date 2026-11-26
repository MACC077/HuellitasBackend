using Huellitas.Infrastructure.Enums;
using Huellitas.Infrastructure.Interfaces.Repositories;
using Huellitas.Infrastructure.Persistence.Context;
using Huellitas.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Huellitas.Infrastructure.Persistence.Repositories;

public class SeguimientoRepository : ISeguimientoRepository
{
    private readonly HuellitasDbContext _context;

    public SeguimientoRepository(HuellitasDbContext context) => _context = context;

    public async Task<PlanesSeguimiento?> ObtenerPlanPorAdopcionAsync(int adopcionId)
    {
        return await _context.PlanesSeguimientos
            .FirstOrDefaultAsync(p => p.AdopcionId == adopcionId && p.Activo);
    }

    public async Task<PlanesSeguimiento?> ObtenerPlanPorIdAsync(int planId)
    {
        return await _context.PlanesSeguimientos
            .FirstOrDefaultAsync(p => p.PlanId == planId && p.Activo);
    }

    public async Task InsertarPlanConSeguimientosAsync(PlanesSeguimiento plan, IEnumerable<Seguimientos> seguimientos)
    {
        foreach (var seguimiento in seguimientos)
            plan.Seguimientos.Add(seguimiento);

        _context.PlanesSeguimientos.Add(plan);
        await _context.SaveChangesAsync();
    }

    public async Task<Seguimientos?> ObtenerSeguimientoPorIdAsync(int seguimientoId)
    {
        return await _context.Seguimientos
            .Include(s => s.Plan)
                .ThenInclude(p => p.Adopcion)
                    .ThenInclude(a => a.Solicitud)
                        .ThenInclude(sol => sol.Mascota)
            .FirstOrDefaultAsync(s => s.SeguimientoId == seguimientoId && s.Activo);
    }

    public async Task ActualizarSeguimientoAsync(Seguimientos seguimiento)
    {
        _context.Seguimientos.Update(seguimiento);
        await _context.SaveChangesAsync();
    }

    public async Task ActualizarSeguimientoConMultimediaAsync(Seguimientos seguimiento, IEnumerable<Multimedia> multimedia)
    {
        _context.Seguimientos.Update(seguimiento);
        _context.Multimedia.AddRange(multimedia);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Seguimientos>> ObtenerSeguimientosPorPlanAsync(int planId)
    {
        return await _context.Seguimientos
            .Include(s => s.Multimedia.Where(m => m.Activo))
            .Where(s => s.PlanId == planId && s.Activo)
            .OrderBy(s => s.Numero)
            .ToListAsync();
    }

    public async Task<IEnumerable<Seguimientos>> ObtenerSeguimientosVencidosAsync(DateOnly fechaLimite)
    {
        return await _context.Seguimientos
            .Include(s => s.Plan)
                .ThenInclude(p => p.Adopcion)
                    .ThenInclude(a => a.Solicitud)
                        .ThenInclude(sol => sol.Mascota)
            .Where(s => s.Estado == (byte)EstadoSeguimiento.Pendiente
                        && s.FechaProgramada < fechaLimite
                        && s.Activo)
            .ToListAsync();
    }
}
