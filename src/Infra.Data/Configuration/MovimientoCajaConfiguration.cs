using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Infra.Data.Context;

namespace TurnosApp.Infra.Data.Configurations;

public class MovimientoCajaConfiguration : IEntityTypeConfiguration<MovimientoCaja>
{
    private readonly ApplicationDbContext _context;

    public MovimientoCajaConfiguration(ApplicationDbContext context)
    {
        _context = context;
    }

    public void Configure(EntityTypeBuilder<MovimientoCaja> builder)
    {
        builder.ToTable("MovimientosCaja");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Tipo)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(m => m.Monto)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(m => m.NombreMetodoPagoSnapshot)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.Concepto)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(m => m.FechaHora)
            .IsRequired();

        // Global Query Filter
        builder.HasQueryFilter(m => m.TenantId == _context.CurrentTenantId);

        builder.HasOne(m => m.Tenant)
            .WithMany(t => t.MovimientosCaja)
            .HasForeignKey(m => m.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.SesionCaja)
            .WithMany(s => s.Movimientos)
            .HasForeignKey(m => m.SesionCajaId)
            .OnDelete(DeleteBehavior.Restrict);

        // SetNull: FK nullable por diseño — si MetodoPago se hard-deletea, el movimiento sobrevive con su snapshot intacto.
        builder.HasOne(m => m.MetodoPago)
            .WithMany()
            .HasForeignKey(m => m.MetodoPagoId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(m => m.Usuario)
            .WithMany()
            .HasForeignKey(m => m.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        // SetNull: si el Cobro se hard-deletea (nunca debería, Cobro es Restrict sobre Turno), el
        // movimiento sobrevive igual que con MetodoPago — es un registro de caja, no depende del Cobro para existir.
        builder.HasOne(m => m.Cobro)
            .WithMany()
            .HasForeignKey(m => m.CobroId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // Self-reference para movimientos de reversa.
        builder.HasOne(m => m.MovimientoOrigen)
            .WithMany()
            .HasForeignKey(m => m.MovimientoOrigenId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
