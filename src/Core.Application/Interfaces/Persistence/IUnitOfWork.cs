using System;
using System.Collections.Generic;
using System.Text;
namespace TurnosApp.Core.Application.Interfaces.Persistence;

/// <summary>
/// Coordina múltiples repositorios dentro de una única transacción de base de datos.
/// Todos los repositorios expuestos aquí comparten el mismo DbContext.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    ITenantRepository Tenants { get; }
    IServicioRepository Servicios { get; }
    IClienteRepository Clientes { get; }
    ITurnoRepository Turnos { get; }
    IRecursoRepository Recursos { get; }

    IUsuarioRepository Usuarios { get; }
    IMetodoPagoRepository MetodoPagos { get; }
    ICobroRepository Cobros { get; }
    IRolRepository Roles { get; }
    IMetricasRepository Metricas { get; }
    IListaEsperaRepository ListasEspera { get; }
    INotificacionRepository Notificaciones { get; }
    IHorarioAtencionRepository HorariosAtencion { get; }
    ISesionCajaRepository SesionesCaja { get; }
    IMovimientoCajaRepository MovimientosCaja { get; }
    IReglaComisionRepository ReglasComision { get; }
    ILiquidacionRepository Liquidaciones { get; }
    ILiquidacionDetalleRepository LiquidacionDetalles { get; }
    IAdelantoProfesionalRepository AdelantosProfesional { get; }
    IPlanRepository Planes { get; }
    IWebhookEventRepository WebhookEvents { get; }

    /// <summary>
    /// Persiste todos los cambios pendientes del change tracker en la base de datos.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}