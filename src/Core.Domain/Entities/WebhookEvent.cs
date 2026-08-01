namespace TurnosApp.Core.Domain.Entities;

using TurnosApp.Core.Domain.Common;

// Outbox de idempotencia para el webhook de Mercado Pago (mismo espíritu que Notificacion,
// pero de entrada en vez de salida): no es TenantEntity porque una notificación de MP
// referencia un preapproval, no un tenant local directamente — hay que resolverlo primero.
public class WebhookEvent : BaseEntity
{
    // El "id" de la notificación en sí (payload.id), no el data.id del recurso que
    // notifica — MP reenvía la misma notificación (mismo id) ante reintentos.
    public string NotificacionId { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public DateTime ProcesadoEn { get; set; } = DateTime.UtcNow;
}
