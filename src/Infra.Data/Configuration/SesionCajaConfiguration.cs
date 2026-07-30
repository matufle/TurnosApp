using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Infra.Data.Context;

namespace TurnosApp.Infra.Data.Configurations;

public class SesionCajaConfiguration : IEntityTypeConfiguration<SesionCaja>
{
    private readonly ApplicationDbContext _context;

    public SesionCajaConfiguration(ApplicationDbContext context)
    {
        _context = context;
    }

    public void Configure(EntityTypeBuilder<SesionCaja> builder)
    {
        builder.ToTable("SesionesCaja");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.MontoInicial)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(s => s.MontoFinalDeclarado)
            .HasPrecision(10, 2);

        builder.Property(s => s.Estado)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(s => s.Observaciones)
            .HasMaxLength(500);

        builder.Property(s => s.FechaApertura)
            .IsRequired();

        // Propiedades [NotMapped] en la entidad — se ignoran explícitamente como defensa en profundidad.
        builder.Ignore(s => s.MontoEsperadoEfectivo);
        builder.Ignore(s => s.Diferencia);

        // Defensa en profundidad además del chequeo explícito en CajaAppService.AbrirSesionAsync:
        // solo puede existir una fila con Estado = 'Abierta' por tenant. Mismo patrón que el
        // índice único filtrado de RecursoConfiguration.UsuarioId.
        builder.HasIndex(s => s.TenantId)
            .IsUnique()
            .HasFilter("\"Estado\" = 'Abierta'");

        // Global Query Filter
        builder.HasQueryFilter(s => s.TenantId == _context.CurrentTenantId);

        builder.HasOne(s => s.Tenant)
            .WithMany(t => t.SesionesCaja)
            .HasForeignKey(s => s.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        // Restrict: una SesionCaja con Movimientos nunca debe poder hard-deletearse (registro financiero/auditable).
        builder.HasOne(s => s.UsuarioApertura)
            .WithMany()
            .HasForeignKey(s => s.UsuarioAperturaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.UsuarioCierre)
            .WithMany()
            .HasForeignKey(s => s.UsuarioCierreId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
