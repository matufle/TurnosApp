using Microsoft.Extensions.Logging;
using TurnosApp.Core.Application.Interfaces.Persistence;
using TurnosApp.Core.Application.Interfaces.Services;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Core.Domain.Enums;
using TurnosApp.Core.Exceptions;

namespace TurnosApp.Core.Application.Services;

public class LiquidacionGeneratorService : ILiquidacionGeneratorService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<LiquidacionGeneratorService> _logger;

    public LiquidacionGeneratorService(IUnitOfWork unitOfWork, ILogger<LiquidacionGeneratorService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task GenerarPendientesAsync(CancellationToken cancellationToken = default)
    {
        // Cross-tenant a propósito: llamado por el worker en background, sin JWT que resolver.
        // Tenant no es TenantEntity (no tiene Global Query Filter), así que GetAllAsync no
        // necesita IgnoreQueryFilters.
        var tenants = await _unitOfWork.Tenants.GetAllAsync(cancellationToken);

        foreach (var tenant in tenants.Where(t => t.Activo))
        {
            try
            {
                await GenerarParaTenantAsync(tenant, cancellationToken);
            }
            catch (Exception ex)
            {
                // Un tenant con datos raros no debe tumbar la corrida de los demás.
                _logger.LogError(ex, "Error generando liquidaciones para el tenant {TenantId}.", tenant.Id);
            }
        }
    }

    public async Task GenerarParaTenantAsync(int tenantId, CancellationToken cancellationToken = default)
    {
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId, cancellationToken);
        if (tenant is null)
            throw new NotFoundException(nameof(Tenant), tenantId);

        await GenerarParaTenantAsync(tenant, cancellationToken);
    }

    private async Task GenerarParaTenantAsync(Tenant tenant, CancellationToken cancellationToken)
    {
        var (periodoDesde, periodoHasta) = CalcularUltimoPeriodoCerrado(tenant.FrecuenciaLiquidacion, DateTime.UtcNow);

        // "Hasta el cierre del período" (no una ventana estricta): un turno rezagado que quedó
        // sin liquidar en un ciclo anterior se suma acá, no se pierde.
        var turnosElegibles = await _unitOfWork.Turnos.GetElegiblesParaLiquidacionCrossTenantAsync(
            tenant.Id, periodoHasta, cancellationToken);
        var adelantosPendientes = await _unitOfWork.AdelantosProfesional.GetPendientesCrossTenantAsync(
            tenant.Id, periodoHasta, cancellationToken);

        var recursoIds = turnosElegibles.Select(t => t.RecursoId)
            .Concat(adelantosPendientes.Select(a => a.RecursoId))
            .Distinct();

        foreach (var recursoId in recursoIds)
        {
            try
            {
                await GenerarParaRecursoAsync(
                    tenant.Id, recursoId, periodoDesde, periodoHasta,
                    turnosElegibles.Where(t => t.RecursoId == recursoId).ToList(),
                    adelantosPendientes.Where(a => a.RecursoId == recursoId).ToList(),
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error generando la liquidación del Recurso {RecursoId} (tenant {TenantId}).",
                    recursoId, tenant.Id);
            }
        }
    }

    private async Task GenerarParaRecursoAsync(
        int tenantId, int recursoId, DateTime periodoDesde, DateTime periodoHasta,
        IReadOnlyList<Turno> turnos, IReadOnlyList<AdelantoProfesional> adelantos,
        CancellationToken cancellationToken)
    {
        // Defensa en profundidad — el índice único de la tabla protege el insert igual,
        // pero chequear antes evita depender de capturar la excepción de duplicado.
        var existente = await _unitOfWork.Liquidaciones.GetByRecursoYPeriodoCrossTenantAsync(
            tenantId, recursoId, periodoDesde, periodoHasta, cancellationToken);
        if (existente is not null)
            return;

        var reglas = await _unitOfWork.ReglasComision.GetVigentesCrossTenantAsync(tenantId, recursoId, cancellationToken);

        var detalles = new List<LiquidacionDetalle>();

        foreach (var turno in turnos)
        {
            foreach (var turnoServicio in turno.TurnoServicios)
            {
                var regla = ResolverRegla(reglas, turnoServicio.ServicioId);

                if (regla is null)
                {
                    // Sin regla configurada (ni por servicio ni base): se excluye de esta
                    // liquidación en vez de zanjar en $0 permanente — queda pendiente hasta
                    // que se configure una regla y lo recoja un próximo ciclo.
                    _logger.LogWarning(
                        "Turno {TurnoId} servicio {ServicioId}: sin regla de comisión para el Recurso {RecursoId} (tenant {TenantId}) — excluido de esta liquidación.",
                        turno.Id, turnoServicio.ServicioId, recursoId, tenantId);
                    continue;
                }

                detalles.Add(new LiquidacionDetalle
                {
                    TurnoId = turno.Id,
                    ServicioId = turnoServicio.ServicioId,
                    PrecioBaseAplicado = turnoServicio.PrecioAplicado,
                    TipoComisionSnapshot = regla.Tipo,
                    ValorComisionSnapshot = regla.Valor
                });
            }
        }

        // Sin turnos liquidables ni adelantos pendientes: no generamos una liquidación vacía.
        if (detalles.Count == 0 && adelantos.Count == 0)
            return;

        var liquidacion = new Liquidacion
        {
            TenantId = tenantId, // asignado a mano: el worker no tiene tenant ambiente que GenericRepository.AddAsync pueda resolver
            RecursoId = recursoId,
            PeriodoDesde = periodoDesde,
            PeriodoHasta = periodoHasta,
            FechaGeneracion = DateTime.UtcNow,
            Estado = EstadoLiquidacion.Generada,
            Detalles = detalles
        };

        await _unitOfWork.Liquidaciones.AddAsync(liquidacion, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken); // asigna Id antes de asociar los adelantos

        foreach (var adelanto in adelantos)
        {
            adelanto.LiquidacionId = liquidacion.Id;
            _unitOfWork.AdelantosProfesional.Update(adelanto);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static ReglaComision? ResolverRegla(IReadOnlyList<ReglaComision> reglas, int servicioId) =>
        reglas.FirstOrDefault(r => r.ServicioId == servicioId) ?? reglas.FirstOrDefault(r => r.ServicioId == null);

    // Calcula el período INMEDIATAMENTE ANTERIOR al que está en curso — el período actual
    // todavía no cerró, no corresponde liquidarlo todavía.
    private static (DateTime Desde, DateTime Hasta) CalcularUltimoPeriodoCerrado(FrecuenciaLiquidacion frecuencia, DateTime ahora)
    {
        switch (frecuencia)
        {
            case FrecuenciaLiquidacion.Semanal:
            {
                // Lunes a domingo ISO.
                var diasDesdeElLunes = (int)ahora.DayOfWeek == 0 ? 6 : (int)ahora.DayOfWeek - 1;
                var inicioSemanaActual = ahora.Date.AddDays(-diasDesdeElLunes);
                var desde = inicioSemanaActual.AddDays(-7);
                var hasta = inicioSemanaActual.AddTicks(-1);
                return (desde, hasta);
            }
            case FrecuenciaLiquidacion.Quincenal:
            {
                if (ahora.Day <= 15)
                {
                    // Todavía en la 1ra quincena: la última cerrada es la 2da del mes anterior.
                    var mesAnterior = ahora.AddMonths(-1);
                    var desde = new DateTime(mesAnterior.Year, mesAnterior.Month, 16);
                    var hasta = new DateTime(ahora.Year, ahora.Month, 1).AddTicks(-1);
                    return (desde, hasta);
                }
                else
                {
                    var desde = new DateTime(ahora.Year, ahora.Month, 1);
                    var hasta = new DateTime(ahora.Year, ahora.Month, 16).AddTicks(-1);
                    return (desde, hasta);
                }
            }
            case FrecuenciaLiquidacion.Mensual:
            default:
            {
                var inicioMesActual = new DateTime(ahora.Year, ahora.Month, 1);
                var desde = inicioMesActual.AddMonths(-1);
                var hasta = inicioMesActual.AddTicks(-1);
                return (desde, hasta);
            }
        }
    }
}
