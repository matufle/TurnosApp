using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using TurnosApp.Core.Domain.Entities;

namespace TurnosApp.Infra.Data.Interceptors;

public class AuditInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
            ApplyAudit(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
            ApplyAudit(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    private static void ApplyAudit(DbContext context)
    {
        var now = DateTime.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries<Turno>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreadoEn = now;
                    break;

                case EntityState.Modified:
                    // Protegemos CreadoEn de ser sobreescrito accidentalmente en updates.
                    entry.Property(t => t.CreadoEn).IsModified = false;
                    entry.Entity.ModificadoEn = now;
                    break;
            }
        }
    }
}