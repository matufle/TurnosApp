using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Infra.Data.Context;

namespace TurnosApp.Infra.Data.Configurations;

public class MetodoPagoConfiguration : IEntityTypeConfiguration<MetodoPago>
{
    private readonly ApplicationDbContext _context;

    public MetodoPagoConfiguration(ApplicationDbContext context)
    {
        _context = context;
    }

    public void Configure(EntityTypeBuilder<MetodoPago> builder)
    {
        builder.ToTable("MetodosPago");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Nombre)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.TipoModificador)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(m => m.PorcentajeModificador)
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(m => m.PorcentajeComision)
            .HasPrecision(5, 2)
            .IsRequired();

        // Global Query Filter
        builder.HasQueryFilter(m => m.TenantId == _context.CurrentTenantId);

        builder.HasOne(m => m.Tenant)
            .WithMany(t => t.MetodosPago)
            .HasForeignKey(m => m.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
