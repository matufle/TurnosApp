using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TurnosApp.Core.Domain.Entities;

namespace TurnosApp.Infra.Data.Configurations;

public class PlanConfiguration : IEntityTypeConfiguration<Plan>
{
    public void Configure(EntityTypeBuilder<Plan> builder)
    {
        builder.ToTable("Planes");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Nombre)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.PrecioMensual)
            .IsRequired()
            .HasColumnType("numeric(12,2)");

        // Plan es catálogo global (como MetodoPago "Efectivo" seedeado por tenant, pero acá
        // uno solo para todos) — sin Global Query Filter, no pertenece a ningún tenant.
    }
}
