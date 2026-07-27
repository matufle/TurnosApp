using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Infra.Data.Context;

namespace TurnosApp.Infra.Data.Configurations;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    private readonly ApplicationDbContext _context;

    public ClienteConfiguration(ApplicationDbContext context)
    {
        _context = context;
    }

    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("Clientes");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Nombre)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(c => c.Apellido)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(c => c.Email)
            .HasMaxLength(200);

        builder.Property(c => c.Telefono)
            .HasMaxLength(30);

        builder.Property(c => c.PasswordHash)
            .HasMaxLength(500);

        // Índice único parcial: sólo exige unicidad de email entre los Cliente que ya
        // tienen cuenta (self-service). Los walk-in cargados por staff sin login pueden
        // seguir repitiendo o no teniendo email, como hoy.
        builder.HasIndex(c => new { c.TenantId, c.Email })
            .IsUnique()
            .HasFilter("\"PasswordHash\" IS NOT NULL AND \"Email\" IS NOT NULL");

        // Columna JSON: nvarchar(max) que almacena el JSON arbitrario por rubro.
        // No usamos .ToJson() (que es para owned entities),
        // sino la columna string directa — máxima flexibilidad sin schema fijo.
        builder.Property(c => c.DatosEspecificosJson)
            .HasColumnType("text")
            .HasColumnName("DatosEspecificosJson");

        // Global Query Filter
        builder.HasQueryFilter(c => c.TenantId == _context.CurrentTenantId);

        builder.HasOne(c => c.Tenant)
            .WithMany(t => t.Clientes)
            .HasForeignKey(c => c.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}