using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using TurnosApp.Core.Application.Interfaces.Services;
using TurnosApp.Core.Application.Services;

namespace TurnosApp.Core.Application.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ITenantAppService, TenantAppService>();    // ← nuevo
        services.AddScoped<IRecursoAppService, RecursoAppService>();
        services.AddScoped<IServicioService, ServicioService>();
        services.AddScoped<IClienteService, ClienteService>();
        services.AddScoped<ITurnoAppService, TurnoAppService>();
        services.AddScoped<IMetodoPagoService, MetodoPagoService>();
        services.AddScoped<ICobroAppService, CobroAppService>();
        services.AddScoped<IRolAppService, RolAppService>();
        services.AddScoped<IUsuarioAppService, UsuarioAppService>();
        services.AddScoped<IMetricasAppService, MetricasAppService>();
        services.AddScoped<IListaEsperaAppService, ListaEsperaAppService>();
        services.AddScoped<INotificacionAppService, NotificacionAppService>();
        services.AddScoped<INotificacionDispatchService, NotificacionDispatchService>();
        services.AddScoped<ICajaAppService, CajaAppService>();
        services.AddScoped<IReglaComisionAppService, ReglaComisionAppService>();
        services.AddScoped<ILiquidacionAppService, LiquidacionAppService>();
        services.AddScoped<IAdelantoProfesionalAppService, AdelantoProfesionalAppService>();
        services.AddScoped<ILiquidacionGeneratorService, LiquidacionGeneratorService>();
        services.AddScoped<ITurnstileService, TurnstileService>();
        services.AddScoped<IMercadoPagoService, MercadoPagoService>();
        services.AddScoped<ISuscripcionAppService, SuscripcionAppService>();

        // SolapamientoValidator como Scoped: necesita ITurnoRepository
        // que también es Scoped (vive dentro del UnitOfWork Scoped).
        services.AddScoped<SolapamientoValidator>();

        return services;
    }
}
