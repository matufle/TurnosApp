using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Infra.Data.Context;

namespace TurnosApp.Infra.Data.Configurations;

public class CobroConfiguration : IEntityTypeConfiguration<Cobro>
{
    private readonly ApplicationDbContext _context;

    public CobroConfiguration(ApplicationDbContext context)
    {
        _context = context;
    }

    public void Configure(EntityTypeBuilder<Cobro> builder)
    {
        builder.ToTable("Cobros");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.NombreMetodoPagoSnapshot)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.TipoModificadorSnapshot)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(c => c.PorcentajeModificadorSnapshot)
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(c => c.PorcentajeComisionSnapshot)
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(c => c.PrecioBase)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(c => c.CreadoEn)
            .IsRequired();

        builder.Property(c => c.CreadoPor)
            .HasMaxLength(100);

        builder.Property(c => c.ModificadoPor)
            .HasMaxLength(100);

        // Propiedades [NotMapped] en la entidad — se ignoran explícitamente como defensa en profundidad.
        builder.Ignore(c => c.MontoModificadorCliente);
        builder.Ignore(c => c.PrecioFinal);
        builder.Ignore(c => c.MontoComision);
        builder.Ignore(c => c.GananciaNeta);

        // Global Query Filter — Cobro hereda TenantEntity directamente (a diferencia de TurnoServicio).
        builder.HasQueryFilter(c => c.TenantId == _context.CurrentTenantId);

        builder.HasOne(c => c.Tenant)
            .WithMany(t => t.Cobros)
            .HasForeignKey(c => c.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        // Restrict: un Turno con Cobros nunca debe poder hard-deletearse (registro financiero/auditable).
        builder.HasOne(c => c.Turno)
            .WithMany(t => t.Cobros)
            .HasForeignKey(c => c.TurnoId)
            .OnDelete(DeleteBehavior.Restrict);

        // SetNull: FK nullable por diseño — si MetodoPago se hard-deletea, el Cobro sobrevive con su snapshot intacto.
        builder.HasOne(c => c.MetodoPago)
            .WithMany(m => m.Cobros)
            .HasForeignKey(c => c.MetodoPagoId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
