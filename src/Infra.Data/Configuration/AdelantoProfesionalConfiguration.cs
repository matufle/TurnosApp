using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Infra.Data.Context;

namespace TurnosApp.Infra.Data.Configurations;

public class AdelantoProfesionalConfiguration : IEntityTypeConfiguration<AdelantoProfesional>
{
    private readonly ApplicationDbContext _context;

    public AdelantoProfesionalConfiguration(ApplicationDbContext context)
    {
        _context = context;
    }

    public void Configure(EntityTypeBuilder<AdelantoProfesional> builder)
    {
        builder.ToTable("AdelantosProfesional");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Monto)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(a => a.Fecha).IsRequired();

        builder.Property(a => a.Concepto)
            .HasMaxLength(300);

        // Global Query Filter
        builder.HasQueryFilter(a => a.TenantId == _context.CurrentTenantId);

        builder.HasOne(a => a.Tenant)
            .WithMany(t => t.AdelantosProfesional)
            .HasForeignKey(a => a.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Recurso)
            .WithMany(r => r.AdelantosProfesional)
            .HasForeignKey(a => a.RecursoId)
            .OnDelete(DeleteBehavior.Restrict);

        // SetNull: anular la Liquidacion libera el adelanto para reconsiderarlo en el próximo ciclo.
        builder.HasOne(a => a.Liquidacion)
            .WithMany(l => l.Adelantos)
            .HasForeignKey(a => a.LiquidacionId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
