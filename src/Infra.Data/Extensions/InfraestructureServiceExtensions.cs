using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TurnosApp.Core.Application.Interfaces.Persistence;
using TurnosApp.Infra.Data.Context;
using TurnosApp.Infra.Data.Interceptors;
using TurnosApp.Infra.Data.Persistence;
using TurnosApp.Infra.Data.Repositories;

namespace TurnosApp.Infra.Data.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // El interceptor como Singleton — no tiene estado mutable.
        services.AddSingleton<AuditInterceptor>();

        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorCodesToAdd: null);

                    npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                });
        });

        // UnitOfWork como Scoped garantiza que una única instancia de DbContext
        // sea compartida por toda la request HTTP, manteniendo la consistencia
        // transaccional sin necesidad de coordinar manualmente.
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ITurnoRepository, TurnoRepository>();
        services.AddScoped<IServicioRepository, ServicioRepository>();
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IRecursoRepository, RecursoRepository>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IMetodoPagoRepository, MetodoPagoRepository>();
        services.AddScoped<ICobroRepository, CobroRepository>();
        services.AddScoped<IRolRepository, RolRepository>();
        services.AddScoped<IMetricasRepository, MetricasRepository>();
        services.AddScoped<IListaEsperaRepository, ListaEsperaRepository>();
        services.AddScoped<INotificacionRepository, NotificacionRepository>();
        services.AddScoped<IHorarioAtencionRepository, HorarioAtencionRepository>();
        services.AddScoped<ISesionCajaRepository, SesionCajaRepository>();
        services.AddScoped<IMovimientoCajaRepository, MovimientoCajaRepository>();
        services.AddScoped<IReglaComisionRepository, ReglaComisionRepository>();
        services.AddScoped<ILiquidacionRepository, LiquidacionRepository>();
        services.AddScoped<ILiquidacionDetalleRepository, LiquidacionDetalleRepository>();
        services.AddScoped<IAdelantoProfesionalRepository, AdelantoProfesionalRepository>();

        // Los repositorios no se registran individualmente en DI —
        // se accede a ellos exclusivamente a través de IUnitOfWork,
        // que los instancia internamente con lazy init compartiendo el DbContext.

        return services;
    }
}