using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Infra.Data.Context;

namespace TurnosApp.Infra.Data.Configurations;

public class RolConfiguration : IEntityTypeConfiguration<Rol>
{
    private readonly ApplicationDbContext _context;

    public RolConfiguration(ApplicationDbContext context)
    {
        _context = context;
    }

    public void Configure(EntityTypeBuilder<Rol> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Nombre)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.PermisosMask)
            .IsRequired();

        builder.Property(r => r.EsSistema)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Ignore(r => r.Permisos);

        // Global Query Filter: todas las queries filtran por el tenant del request actual.
        builder.HasQueryFilter(r => r.TenantId == _context.CurrentTenantId);

        builder.HasOne(r => r.Tenant)
            .WithMany()
            .HasForeignKey(r => r.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
