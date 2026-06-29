using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Core.Domain.Enums;
using TurnosApp.Infra.Data.Context;

namespace TurnosApp.Infra.Data.Configurations;

public class TurnoConfiguration : IEntityTypeConfiguration<Turno>
{
    private readonly ApplicationDbContext _context;

    public TurnoConfiguration(ApplicationDbContext context)
    {
        _context = context;
    }

    public void Configure(EntityTypeBuilder<Turno> builder)
    {
        builder.ToTable("Turnos");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.FechaHoraInicio)
            .IsRequired();

        // Ignorar explícitamente la propiedad calculada — [NotMapped] ya lo hace,
        // pero esto es defensa en profundidad y documenta la intención en la capa de datos.
        builder.Ignore(t => t.FechaHoraFin);

        // Estado persiste como string para legibilidad en la DB y resiliencia ante
        // futuros reordenamientos del enum.
        builder.Property(t => t.Estado)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(EstadoTurno.Pendiente);

        builder.Property(t => t.Notas)
            .HasMaxLength(1000);

        builder.Property(t => t.CreadoEn)
            .IsRequired();

        builder.Property(t => t.CreadoPor)
            .HasMaxLength(100);

        // Índice compuesto para acelerar consultas de solapamiento por recurso y fecha.
        builder.HasIndex(t => new { t.RecursoId, t.FechaHoraInicio })
            .HasDatabaseName("IX_Turnos_Recurso_FechaHoraInicio");

        // Global Query Filter
        builder.HasQueryFilter(t => t.TenantId == _context.CurrentTenantId);

        builder.HasOne(t => t.Tenant)
            .WithMany(te => te.Turnos)
            .HasForeignKey(t => t.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Recurso)
            .WithMany(r => r.Turnos)
            .HasForeignKey(t => t.RecursoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Cliente)
            .WithMany(c => c.Turnos)
            .HasForeignKey(t => t.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
