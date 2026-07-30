using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Infra.Data.Context;

namespace TurnosApp.Infra.Data.Configurations;

public class LiquidacionConfiguration : IEntityTypeConfiguration<Liquidacion>
{
    private readonly ApplicationDbContext _context;

    public LiquidacionConfiguration(ApplicationDbContext context)
    {
        _context = context;
    }

    public void Configure(EntityTypeBuilder<Liquidacion> builder)
    {
        builder.ToTable("Liquidaciones");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Estado)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(l => l.Observaciones)
            .HasMaxLength(500);

        builder.Property(l => l.PeriodoDesde).IsRequired();
        builder.Property(l => l.PeriodoHasta).IsRequired();
        builder.Property(l => l.FechaGeneracion).IsRequired();

        // Propiedades [NotMapped] — se ignoran explícitamente como defensa en profundidad,
        // mismo patrón que SesionCajaConfiguration con MontoEsperadoEfectivo/Diferencia.
        builder.Ignore(l => l.MontoBrutoComision);
        builder.Ignore(l => l.MontoAdelantos);
        builder.Ignore(l => l.MontoNeto);

        // Global Query Filter
        builder.HasQueryFilter(l => l.TenantId == _context.CurrentTenantId);

        builder.HasOne(l => l.Tenant)
            .WithMany(t => t.Liquidaciones)
            .HasForeignKey(l => l.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.Recurso)
            .WithMany(r => r.Liquidaciones)
            .HasForeignKey(l => l.RecursoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.UsuarioPago)
            .WithMany()
            .HasForeignKey(l => l.UsuarioPagoId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // Defensa en profundidad contra doble generación del mismo período para el mismo
        // profesional (el worker igual chequea existencia antes de insertar).
        builder.HasIndex(l => new { l.TenantId, l.RecursoId, l.PeriodoDesde, l.PeriodoHasta })
            .IsUnique();
    }
}
