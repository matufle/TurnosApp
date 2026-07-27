using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Infra.Data.Context;

namespace TurnosApp.Infra.Data.Configurations;

public class ListaEsperaConfiguration : IEntityTypeConfiguration<ListaEspera>
{
    private readonly ApplicationDbContext _context;

    public ListaEsperaConfiguration(ApplicationDbContext context)
    {
        _context = context;
    }

    public void Configure(EntityTypeBuilder<ListaEspera> builder)
    {
        builder.ToTable("ListasEspera");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Estado)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.HasOne(l => l.Cliente)
            .WithMany()
            .HasForeignKey(l => l.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.Recurso)
            .WithMany()
            .HasForeignKey(l => l.RecursoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.Servicio)
            .WithMany()
            .HasForeignKey(l => l.ServicioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(l => new { l.RecursoId, l.Estado });

        // Global Query Filter
        builder.HasQueryFilter(l => l.TenantId == _context.CurrentTenantId);

        builder.HasOne(l => l.Tenant)
            .WithMany()
            .HasForeignKey(l => l.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
