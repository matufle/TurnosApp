using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Infra.Data.Context;

namespace TurnosApp.Infra.Data.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Nombre)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Slug)
            .IsRequired()
            .HasMaxLength(100);

        // El slug debe ser único globalmente — es el identificador público del tenant.
        builder.HasIndex(t => t.Slug)
            .IsUnique();

        builder.Property(t => t.FechaAlta)
            .IsRequired();

        // Tenant no tiene Global Query Filter — es la raíz del multi-tenancy,
        // no pertenece a ningún tenant.
    }
}
