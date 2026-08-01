using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TurnosApp.Core.Domain.Entities;

namespace TurnosApp.Infra.Data.Configurations;

public class WebhookEventConfiguration : IEntityTypeConfiguration<WebhookEvent>
{
    public void Configure(EntityTypeBuilder<WebhookEvent> builder)
    {
        builder.ToTable("WebhookEvents");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.NotificacionId)
            .IsRequired()
            .HasMaxLength(255);

        // Idempotencia ante reintentos de Mercado Pago: la misma notificación no se procesa dos veces.
        builder.HasIndex(w => w.NotificacionId)
            .IsUnique();

        builder.Property(w => w.Tipo)
            .IsRequired()
            .HasMaxLength(100);
    }
}
