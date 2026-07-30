using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Infra.Data.Context;

namespace TurnosApp.Infra.Data.Configurations;

public class ReglaComisionConfiguration : IEntityTypeConfiguration<ReglaComision>
{
    private readonly ApplicationDbContext _context;

    public ReglaComisionConfiguration(ApplicationDbContext context)
    {
        _context = context;
    }

    public void Configure(EntityTypeBuilder<ReglaComision> builder)
    {
        builder.ToTable("ReglasComision");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Tipo)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(r => r.Valor)
            .HasPrecision(10, 2)
            .IsRequired();

        // Global Query Filter
        builder.HasQueryFilter(r => r.TenantId == _context.CurrentTenantId);

        builder.HasOne(r => r.Tenant)
            .WithMany(t => t.ReglasComision)
            .HasForeignKey(r => r.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Recurso)
            .WithMany(rec => rec.ReglasComision)
            .HasForeignKey(r => r.RecursoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Servicio)
            .WithMany(s => s.ReglasComision)
            .HasForeignKey(r => r.ServicioId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // Postgres no considera iguales dos NULL a los fines de unicidad, por eso la regla
        // "base" (ServicioId NULL) y las override puntuales necesitan un índice cada una:
        // override por servicio — a lo sumo una regla activa por (Recurso, Servicio).
        builder.HasIndex(r => new { r.TenantId, r.RecursoId, r.ServicioId })
            .IsUnique()
            .HasFilter("\"Activo\" = true AND \"ServicioId\" IS NOT NULL");

        // regla base — a lo sumo una regla activa sin ServicioId por Recurso.
        builder.HasIndex(r => new { r.TenantId, r.RecursoId })
            .IsUnique()
            .HasFilter("\"Activo\" = true AND \"ServicioId\" IS NULL");
    }
}
