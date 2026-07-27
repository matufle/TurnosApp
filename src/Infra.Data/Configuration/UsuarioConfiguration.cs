// TurnosApp.Infra.Data/Configurations/UsuarioConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Infra.Data.Context;

namespace TurnosApp.Infra.Data.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    private readonly ApplicationDbContext _context;

    public UsuarioConfiguration(ApplicationDbContext context)
    {
        _context = context;
    }

    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Nombre)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(u => u.PasswordHash)
            .IsRequired();

        builder.Property(u => u.TenantId)
            .IsRequired();

        builder.Property(u => u.RolId)
            .IsRequired();

        builder.Property(u => u.Activo)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(u => u.OnboardingCompletado)
            .IsRequired()
            .HasDefaultValue(false);

        // Restrict: no se puede borrar un Rol que todavía tiene usuarios asignados
        // (misma regla ya se valida antes en RolAppService, esto es la red de seguridad a nivel DB).
        builder.HasOne(u => u.Rol)
            .WithMany(r => r.Usuarios)
            .HasForeignKey(u => u.RolId)
            .OnDelete(DeleteBehavior.Restrict);

        // Único GLOBAL (no compuesto con TenantId): el login busca por email
        // antes de conocer el tenant, así que dos tenants no pueden compartir email.
        builder.HasIndex(u => u.Email)
            .IsUnique();

        // Mismo patrón que Recurso: filtra por el tenant del request actual.
        // El login lo saltea explícitamente con IgnoreQueryFilters() en el repositorio,
        // porque en ese momento _context.CurrentTenantId todavía no está resuelto.
        builder.HasQueryFilter(u => u.TenantId == _context.CurrentTenantId);

        // Sin navegación a Tenant (Usuario solo tiene el TenantId como FK simple).
        // EF Core igual crea la FK a nivel de base de datos con esta configuración:
        builder.HasOne<TurnosApp.Core.Domain.Entities.Tenant>()
            .WithMany()
            .HasForeignKey(u => u.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
