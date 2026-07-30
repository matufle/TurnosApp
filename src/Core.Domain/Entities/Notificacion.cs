namespace TurnosApp.Core.Domain.Entities;

using TurnosApp.Core.Domain.Common;
using TurnosApp.Core.Domain.Enums;

public class Notificacion : TenantEntity
{
    public TipoNotificacion Tipo { get; set; }
    public EstadoEnvioNotificacion EstadoEnvio { get; set; } = EstadoEnvioNotificacion.Pendiente;

    public int? TurnoId { get; set; }
    public Turno? Turno { get; set; }

    public int? ListaEsperaId { get; set; }
    public ListaEspera? ListaEspera { get; set; }

    // Nullable: outbox pensado originalmente para Cliente únicamente; se reusa para las
    // notificaciones de confirmación de email de Usuario, que no tiene concepto de Cliente.
    public int? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    // Snapshot congelado al programar la notificación (patrón historical snapshot): el
    // worker despacha estos valores tal cual, sin volver a leer Turno/Cliente/Recurso.
    public string DestinatarioEmail { get; set; } = string.Empty;
    public string Asunto { get; set; } = string.Empty;
    public string CuerpoHtml { get; set; } = string.Empty;

    public DateTime ProgramadaPara { get; set; }
    public DateTime? EnviadaEn { get; set; }
    public int IntentosEnvio { get; set; } = 0;
    public string? UltimoError { get; set; }

    // Bookkeeping del claim atómico que hace el worker antes de despachar un lote.
    public DateTime? ReclamadaEn { get; set; }
    public Guid? ReclamadaPorRunId { get; set; }

    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
    public DateTime? ModificadoEn { get; set; }
}
