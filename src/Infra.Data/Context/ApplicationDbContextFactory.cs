// src/Infra.Data/Context/ApplicationDbContextFactory.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using TurnosApp.Infra.Data.Interceptors;

namespace TurnosApp.Infra.Data.Context;

/// <summary>
/// Usada exclusivamente por EF Core Tools en tiempo de diseño
/// (dotnet ef migrations add / database update).
/// Nunca se invoca en producción — el DI de Program.cs toma el control allí.
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // Construye la configuración leyendo appsettings del proyecto WebAPI.
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(),
                         "../Presentation.WebAPI"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        optionsBuilder.UseSqlServer(
            configuration.GetConnectionString("DefaultConnection"),
            sql => sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));

        // TenantProvider stub para diseño: las migraciones no necesitan
        // un tenant real, pero el DbContext lo requiere por constructor.
        return new ApplicationDbContext(
            optionsBuilder.Options,
            new DesignTimeTenantProvider(),
            new AuditInterceptor());
    }
}

/// <summary>
/// Implementación mínima de ITenantProvider para que el DbContext
/// pueda instanciarse durante la generación de migraciones.
/// Retorna 0 — los Global Query Filters no se evalúan en migraciones.
/// </summary>
internal sealed class DesignTimeTenantProvider
    : TurnosApp.Core.Application.Interfaces.ITenantProvider
{
    public int GetCurrentTenantId() => 0;
}