using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TurnosApp.Core.Domain.Entities;

namespace TurnosApp.Infra.Data.Configurations;

public class LiquidacionDetalleConfiguration : IEntityTypeConfiguration<LiquidacionDetalle>
{
    public void Configure(EntityTypeBuilder<LiquidacionDetalle> builder)
    {
        builder.ToTable("LiquidacionDetalles");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.PrecioBaseAplicado)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(d => d.TipoComisionSnapshot)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(d => d.ValorComisionSnapshot)
            .HasPrecision(10, 2)
            .IsRequired();

        // [NotMapped] en la entidad — ignorado explícitamente como defensa en profundidad.
        builder.Ignore(d => d.MontoComisionCalculado);

        builder.HasOne(d => d.Liquidacion)
            .WithMany(l => l.Detalles)
            .HasForeignKey(d => d.LiquidacionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict: un Turno con detalles de liquidación nunca debe poder hard-deletearse
        // (registro financiero/auditable), mismo criterio que CobroConfiguration sobre Turno.
        builder.HasOne(d => d.Turno)
            .WithMany(t => t.LiquidacionDetalles)
            .HasForeignKey(d => d.TurnoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.Servicio)
            .WithMany()
            .HasForeignKey(d => d.ServicioId)
            .OnDelete(DeleteBehavior.Restrict);

        // LiquidacionDetalle no tiene TenantId propio — hereda el aislamiento a través de
        // Liquidacion, que ya tiene Global Query Filter. No aplicamos filtro redundante aquí.
    }
}
