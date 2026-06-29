using Microsoft.EntityFrameworkCore;
using TurnosApp.Core.Application.Interfaces;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Infra.Data.Configurations;
using TurnosApp.Infra.Data.Interceptors;

namespace TurnosApp.Infra.Data.Context;

public class ApplicationDbContext : DbContext
{
    private readonly ITenantProvider _tenantProvider;
    private readonly AuditInterceptor _auditInterceptor;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ITenantProvider tenantProvider,
        AuditInterceptor auditInterceptor)
        : base(options)
    {
        _tenantProvider = tenantProvider;
        _auditInterceptor = auditInterceptor;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Recurso> Recursos => Set<Recurso>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Servicio> Servicios => Set<Servicio>();
    public DbSet<Turno> Turnos => Set<Turno>();
    public DbSet<TurnoServicio> TurnoServicios => Set<TurnoServicio>();

    // Exponemos el TenantId resuelto para que las configuraciones lo capturen
    // en el closure del Global Query Filter.
    internal int CurrentTenantId => _tenantProvider.GetCurrentTenantId();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_auditInterceptor);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Factory que inyecta 'this' en las configuraciones que lo requieran.
        modelBuilder.ApplyConfigurationsFromAssembly(
            assembly: typeof(ApplicationDbContext).Assembly,
            predicate: type => type.Namespace?.Contains("Configurations") == true
        );

        // Para las configuraciones que necesitan el contexto, las registramos manualmente.
        // ApplyConfigurationsFromAssembly solo tomará las que tengan constructor vacío.
        modelBuilder.ApplyConfiguration(new RecursoConfiguration(this));
        modelBuilder.ApplyConfiguration(new ClienteConfiguration(this));
        modelBuilder.ApplyConfiguration(new ServicioConfiguration(this));
        modelBuilder.ApplyConfiguration(new TurnoConfiguration(this));

        // Estas no necesitan contexto — se registran solas vía assembly scan:
        // TenantConfiguration, TurnoServicioConfiguration

        base.OnModelCreating(modelBuilder);
    }
}