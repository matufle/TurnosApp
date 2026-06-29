using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Infra.Data.Context;

namespace TurnosApp.Infra.Data.Configurations;

public class RecursoConfiguration : IEntityTypeConfiguration<Recurso>
{
    private readonly ApplicationDbContext _context;

    public RecursoConfiguration(ApplicationDbContext context)
    {
        _context = context;
    }

    public void Configure(EntityTypeBuilder<Recurso> builder)
    {
        builder.ToTable("Recursos");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Nombre)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.Descripcion)
            .HasMaxLength(500);

        // Global Query Filter: todas las queries filtran por el tenant del request actual.
        builder.HasQueryFilter(r => r.TenantId == _context.CurrentTenantId);

        builder.HasOne(r => r.Tenant)
            .WithMany(t => t.Recursos)
            .HasForeignKey(r => r.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}