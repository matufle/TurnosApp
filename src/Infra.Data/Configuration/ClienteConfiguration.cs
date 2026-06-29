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

        // Columna JSON: nvarchar(max) que almacena el JSON arbitrario por rubro.
        // No usamos .ToJson() (que es para owned entities),
        // sino la columna string directa — máxima flexibilidad sin schema fijo.
        builder.Property(c => c.DatosEspecificosJson)
            .HasColumnType("nvarchar(max)")
            .HasColumnName("DatosEspecificosJson");

        // Global Query Filter
        builder.HasQueryFilter(c => c.TenantId == _context.CurrentTenantId);

        builder.HasOne(c => c.Tenant)
            .WithMany(t => t.Clientes)
            .HasForeignKey(c => c.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}