using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TurnosApp.Core.Domain.Entities;

namespace TurnosApp.Infra.Data.Configurations;

public class TurnoServicioConfiguration : IEntityTypeConfiguration<TurnoServicio>
{
    public void Configure(EntityTypeBuilder<TurnoServicio> builder)
    {
        builder.ToTable("TurnoServicios");

        // Clave primaria compuesta: un servicio no puede aparecer dos veces en el mismo turno.
        builder.HasKey(ts => new { ts.TurnoId, ts.ServicioId });

        builder.Property(ts => ts.Orden)
            .IsRequired();

        builder.Property(ts => ts.PrecioAplicado)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.HasOne(ts => ts.Turno)
            .WithMany(t => t.TurnoServicios)
            .HasForeignKey(ts => ts.TurnoId)
            .OnDelete(DeleteBehavior.Cascade);   // Si se elimina el Turno, se eliminan sus servicios

        builder.HasOne(ts => ts.Servicio)
            .WithMany(s => s.TurnoServicios)
            .HasForeignKey(ts => ts.ServicioId)
            .OnDelete(DeleteBehavior.Restrict);  // No eliminar un Servicio si tiene turnos históricos

        // TurnoServicio no tiene TenantId propio — hereda el aislamiento a través del Turno,
        // que ya tiene Global Query Filter. No aplicamos filtro redundante aquí.
    }
}