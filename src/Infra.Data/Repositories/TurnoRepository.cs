using Microsoft.EntityFrameworkCore;
using TurnosApp.Core.Application.Interfaces.Persistence;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Core.Domain.Enums;
using TurnosApp.Infra.Data.Context;
using TurnosApp.Infra.Data.Repositories;

public class TurnoRepository : GenericRepository<Turno>, ITurnoRepository
{
    public TurnoRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<bool> ExisteTurnoEnRangoAsync(
        int recursoId, DateTime inicio, DateTime fin,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t =>
                t.RecursoId == recursoId &&
                (t.Estado == EstadoTurno.Pendiente || t.Estado == EstadoTurno.Confirmado))
            .Select(t => new
            {
                t.FechaHoraInicio,
                FechaHoraFin = t.FechaHoraInicio.AddMinutes(
                    t.TurnoServicios.Sum(ts => ts.Servicio!.DuracionMinutos))
            })
            .Where(t => inicio < t.FechaHoraFin && fin > t.FechaHoraInicio)
            .AnyAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Turno>> GetAllConDetallesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(t => t.Recurso)
            .Include(t => t.Cliente)
            .Include(t => t.TurnoServicios)
                .ThenInclude(ts => ts.Servicio)
            .Include(t => t.Cobros)
            .ToListAsync(cancellationToken);
    }

    public async Task<Turno?> GetByIdConDetallesAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.Recurso)
            .Include(t => t.Cliente)
            .Include(t => t.TurnoServicios)
                .ThenInclude(ts => ts.Servicio)
            .Include(t => t.Cobros)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Turno>> GetTurnosDelDiaCrossTenantAsync(
        int tenantId, int recursoId, DateOnly fecha, CancellationToken cancellationToken = default)
    {
        var inicioDia = fecha.ToDateTime(TimeOnly.MinValue);
        var finDia = inicioDia.AddDays(1);

        // Cross-tenant a propósito — mismo motivo que Notificacion/Cliente/HorarioAtencion:
        // se llama desde el cálculo de disponibilidad público, sin JWT que resolver. Mismo
        // filtro de estados "activos" que ExisteTurnoEnRangoAsync, para que la disponibilidad
        // mostrada nunca contradiga lo que el propio SolapamientoValidator terminaría aceptando.
        return await _dbSet
            .IgnoreQueryFilters()
            .Include(t => t.TurnoServicios)
                .ThenInclude(ts => ts.Servicio)
            .Where(t =>
                t.TenantId == tenantId &&
                t.RecursoId == recursoId &&
                (t.Estado == EstadoTurno.Pendiente || t.Estado == EstadoTurno.Confirmado) &&
                t.FechaHoraInicio >= inicioDia && t.FechaHoraInicio < finDia)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Turno>> GetElegiblesParaLiquidacionCrossTenantAsync(
        int tenantId, DateTime hasta, CancellationToken cancellationToken = default)
    {
        // Cross-tenant a propósito: lo llama el worker de liquidaciones, sin JWT que resolver.
        var candidatos = await _dbSet
            .IgnoreQueryFilters()
            .Include(t => t.TurnoServicios)
            .Include(t => t.Cobros)
            .Include(t => t.LiquidacionDetalles)
                .ThenInclude(d => d.Liquidacion)
            .Where(t =>
                t.TenantId == tenantId &&
                t.Estado == EstadoTurno.Completado &&
                t.FechaHoraInicio <= hasta)
            .ToListAsync(cancellationToken);

        // "Completamente pagado" y "sin liquidar vigente" no se pueden traducir a SQL sobre
        // columnas [NotMapped] — se filtra en memoria sobre el set ya acotado por
        // tenant/estado/fecha (mismo criterio ya documentado para EstadoPago en Métricas).
        return candidatos
            .Where(t =>
            {
                var precioTotal = t.TurnoServicios.Sum(ts => ts.PrecioAplicado);
                var montoCobrado = t.Cobros.Sum(c => c.PrecioBase);
                var completamentePagado = montoCobrado >= precioTotal;

                var yaLiquidado = t.LiquidacionDetalles.Any(d => d.Liquidacion.Estado != EstadoLiquidacion.Anulada);

                return completamentePagado && !yaLiquidado;
            })
            .ToList();
    }
}