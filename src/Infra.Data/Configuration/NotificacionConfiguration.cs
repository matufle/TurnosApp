using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Infra.Data.Context;

namespace TurnosApp.Infra.Data.Configurations;

public class NotificacionConfiguration : IEntityTypeConfiguration<Notificacion>
{
    private readonly ApplicationDbContext _context;

    public NotificacionConfiguration(ApplicationDbContext context)
    {
        _context = context;
    }

    public void Configure(EntityTypeBuilder<Notificacion> builder)
    {
        builder.ToTable("Notificaciones");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Tipo)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(n => n.EstadoEnvio)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(n => n.DestinatarioEmail).IsRequired().HasMaxLength(320);
        builder.Property(n => n.Asunto).IsRequired().HasMaxLength(200);
        builder.Property(n => n.CuerpoHtml).IsRequired();
        builder.Property(n => n.UltimoError).HasMaxLength(1000);

        builder.HasOne(n => n.Turno)
            .WithMany()
            .HasForeignKey(n => n.TurnoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(n => n.ListaEspera)
            .WithMany()
            .HasForeignKey(n => n.ListaEsperaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(n => n.Cliente)
            .WithMany()
            .HasForeignKey(n => n.ClienteId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(n => new { n.EstadoEnvio, n.ProgramadaPara });
        builder.HasIndex(n => new { n.TurnoId, n.EstadoEnvio });
        builder.HasIndex(n => n.ReclamadaPorRunId);

        // Global Query Filter
        builder.HasQueryFilter(n => n.TenantId == _context.CurrentTenantId);

        builder.HasOne(n => n.Tenant)
            .WithMany()
            .HasForeignKey(n => n.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
