using TurnosApp.Core.Domain.Entities;

namespace TurnosApp.Core.Application.Interfaces.Persistence;

/// <summary>
/// Repositorio de WebhookEvent. Sin Global Query Filter (no es TenantEntity, ver la
/// nota en la entidad sobre por qué).
/// </summary>
public interface IWebhookEventRepository : IRepository<WebhookEvent>
{
    Task<WebhookEvent?> GetByNotificacionIdAsync(string notificacionId, CancellationToken cancellationToken = default);
}
