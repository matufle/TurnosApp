using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Infra.Data.Context;

namespace TurnosApp.Infra.Data.Configurations;

public class ServicioConfiguration : IEntityTypeConfiguration<Servicio>
{
    private readonly ApplicationDbContext _context;

    public ServicioConfiguration(ApplicationDbContext context)
    {
        _context = context;
    }

    public void Configure(EntityTypeBuilder<Servicio> builder)
    {
        builder.ToTable("Servicios");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Nombre)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Descripcion)
            .HasMaxLength(500);

        builder.Property(s => s.DuracionMinutos)
            .IsRequired();

        builder.Property(s => s.Precio)
            .HasPrecision(10, 2)
            .IsRequired();

        // Global Query Filter
        builder.HasQueryFilter(s => s.TenantId == _context.CurrentTenantId);

        builder.HasOne(s => s.Tenant)
            .WithMany(t => t.Servicios)
            .HasForeignKey(s => s.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}