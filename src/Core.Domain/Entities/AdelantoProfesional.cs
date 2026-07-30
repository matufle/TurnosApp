using TurnosApp.Core.Domain.Common;

namespace TurnosApp.Core.Domain.Entities;

// Carga manual de un adelanto/retiro a cuenta del profesional (desacoplado de Caja por ahora).
// LiquidacionId queda null hasta que una Liquidacion cuyo período cubra Fecha lo absorba —
// al crearse, si ya existe una Generada (no Pagada) que lo cubra, se asigna al vuelo; si no,
// lo recoge el worker en la próxima generación. Anular una Liquidacion libera sus adelantos
// (vuelven a LiquidacionId = null) para que se reconsideren en el próximo ciclo.
public class AdelantoProfesional : TenantEntity
{
    public int RecursoId { get; set; }

    public decimal Monto { get; set; }
    public DateTime Fecha { get; set; }
    public string? Concepto { get; set; }

    public int? LiquidacionId { get; set; }

    // Navegación
    public Recurso Recurso { get; set; } = null!;
    public Liquidacion? Liquidacion { get; set; }
}
