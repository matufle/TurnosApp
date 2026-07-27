using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Infra.Data.Context;

namespace TurnosApp.Infra.Data.Configurations;

public class HorarioAtencionConfiguration : IEntityTypeConfiguration<HorarioAtencion>
{
    private readonly ApplicationDbContext _context;

    public HorarioAtencionConfiguration(ApplicationDbContext context)
    {
        _context = context;
    }

    public void Configure(EntityTypeBuilder<HorarioAtencion> builder)
    {
        builder.ToTable("HorariosAtencion");

        builder.HasKey(h => h.Id);

        builder.HasOne(h => h.Recurso)
            .WithMany()
            .HasForeignKey(h => h.RecursoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(h => new { h.RecursoId, h.DiaSemana });

        // Global Query Filter
        builder.HasQueryFilter(h => h.TenantId == _context.CurrentTenantId);

        builder.HasOne(h => h.Tenant)
            .WithMany()
            .HasForeignKey(h => h.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
