using System;
using System.Collections.Generic;
using System.Text;
using TurnosApp.Core.Application.Interfaces.Persistence;
using TurnosApp.Infra.Data.Context;
using TurnosApp.Infra.Data.Repositories;

namespace TurnosApp.Infra.Data.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    // Repositorios recibidos por DI — misma instancia Scoped que el DbContext.
    public ITenantRepository Tenants { get; }
    public IServicioRepository Servicios { get; }
    public IClienteRepository Clientes { get; }
    public ITurnoRepository Turnos { get; }
    public IRecursoRepository Recursos { get; }
    public IUsuarioRepository Usuarios { get; }
    public IMetodoPagoRepository MetodoPagos { get; }
    public ICobroRepository Cobros { get; }
    public IRolRepository Roles { get; }
    public IMetricasRepository Metricas { get; }
    public IListaEsperaRepository ListasEspera { get; }
    public INotificacionRepository Notificaciones { get; }
    public IHorarioAtencionRepository HorariosAtencion { get; }
    public ISesionCajaRepository SesionesCaja { get; }
    public IMovimientoCajaRepository MovimientosCaja { get; }
    public IReglaComisionRepository ReglasComision { get; }
    public ILiquidacionRepository Liquidaciones { get; }
    public ILiquidacionDetalleRepository LiquidacionDetalles { get; }
    public IAdelantoProfesionalRepository AdelantosProfesional { get; }
    public IPlanRepository Planes { get; }
    public IWebhookEventRepository WebhookEvents { get; }

    public UnitOfWork(
        ApplicationDbContext context,
        ITenantRepository tenants,
        IServicioRepository servicios,
        IClienteRepository clientes,
        IRecursoRepository recursos,
        IUsuarioRepository usuarios,
        ITurnoRepository turnos,
        IMetodoPagoRepository metodoPagos,
        ICobroRepository cobros,
        IRolRepository roles,
        IMetricasRepository metricas,
        IListaEsperaRepository listasEspera,
        INotificacionRepository notificaciones,
        IHorarioAtencionRepository horariosAtencion,
        ISesionCajaRepository sesionesCaja,
        IMovimientoCajaRepository movimientosCaja,
        IReglaComisionRepository reglasComision,
        ILiquidacionRepository liquidaciones,
        ILiquidacionDetalleRepository liquidacionDetalles,
        IAdelantoProfesionalRepository adelantosProfesional,
        IPlanRepository planes,
        IWebhookEventRepository webhookEvents)
    {
        _context = context;
        Tenants = tenants;
        Servicios = servicios;
        Clientes = clientes;
        Turnos = turnos;
        Recursos = recursos;
        Usuarios = usuarios;
        MetodoPagos = metodoPagos;
        Cobros = cobros;
        Roles = roles;
        Metricas = metricas;
        ListasEspera = listasEspera;
        Notificaciones = notificaciones;
        HorariosAtencion = horariosAtencion;
        SesionesCaja = sesionesCaja;
        MovimientosCaja = movimientosCaja;
        ReglasComision = reglasComision;
        Liquidaciones = liquidaciones;
        LiquidacionDetalles = liquidacionDetalles;
        AdelantosProfesional = adelantosProfesional;
        Planes = planes;
        WebhookEvents = webhookEvents;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);

    public void Dispose()
        => _context.Dispose();
}