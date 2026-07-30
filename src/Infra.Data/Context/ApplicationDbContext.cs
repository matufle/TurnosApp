using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TurnosApp.Core.Application.Interfaces.Services;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Infra.Data.Configurations;
using TurnosApp.Infra.Data.Conversions;
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

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<MetodoPago> MetodosPago => Set<MetodoPago>();
    public DbSet<Cobro> Cobros => Set<Cobro>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<ListaEspera> ListasEspera => Set<ListaEspera>();
    public DbSet<Notificacion> Notificaciones => Set<Notificacion>();
    public DbSet<HorarioAtencion> HorariosAtencion => Set<HorarioAtencion>();
    public DbSet<SesionCaja> SesionesCaja => Set<SesionCaja>();
    public DbSet<MovimientoCaja> MovimientosCaja => Set<MovimientoCaja>();
    public DbSet<ReglaComision> ReglasComision => Set<ReglaComision>();
    public DbSet<Liquidacion> Liquidaciones => Set<Liquidacion>();
    public DbSet<LiquidacionDetalle> LiquidacionDetalles => Set<LiquidacionDetalle>();
    public DbSet<AdelantoProfesional> AdelantosProfesional => Set<AdelantoProfesional>();

    // Exponemos el TenantId resuelto para que las configuraciones lo capturen
    // en el closure del Global Query Filter.
    internal int CurrentTenantId => _tenantProvider.GetCurrentTenantId();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_auditInterceptor);
    }
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
        configurationBuilder.Properties<DateTime?>().HaveConversion<UtcNullableDateTimeConverter>();

        base.ConfigureConventions(configurationBuilder);
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
        modelBuilder.ApplyConfiguration(new UsuarioConfiguration(this));
        modelBuilder.ApplyConfiguration(new MetodoPagoConfiguration(this));
        modelBuilder.ApplyConfiguration(new CobroConfiguration(this));
        modelBuilder.ApplyConfiguration(new RolConfiguration(this));
        modelBuilder.ApplyConfiguration(new ListaEsperaConfiguration(this));
        modelBuilder.ApplyConfiguration(new NotificacionConfiguration(this));
        modelBuilder.ApplyConfiguration(new HorarioAtencionConfiguration(this));
        modelBuilder.ApplyConfiguration(new SesionCajaConfiguration(this));
        modelBuilder.ApplyConfiguration(new MovimientoCajaConfiguration(this));
        modelBuilder.ApplyConfiguration(new ReglaComisionConfiguration(this));
        modelBuilder.ApplyConfiguration(new LiquidacionConfiguration(this));
        modelBuilder.ApplyConfiguration(new AdelantoProfesionalConfiguration(this));
        // Estas no necesitan contexto — se registran solas vía assembly scan:
        // TenantConfiguration, TurnoServicioConfiguration, LiquidacionDetalleConfiguration

        base.OnModelCreating(modelBuilder);
    }
}