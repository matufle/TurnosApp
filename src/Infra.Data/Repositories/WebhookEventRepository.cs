using Microsoft.EntityFrameworkCore;
using TurnosApp.Core.Application.Interfaces.Persistence;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Infra.Data.Context;

namespace TurnosApp.Infra.Data.Repositories;

public class WebhookEventRepository : GenericRepository<WebhookEvent>, IWebhookEventRepository
{
    public WebhookEventRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<WebhookEvent?> GetByNotificacionIdAsync(string notificacionId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(w => w.NotificacionId == notificacionId, cancellationToken);
    }
}
