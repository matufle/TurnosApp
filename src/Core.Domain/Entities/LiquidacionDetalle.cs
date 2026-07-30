using System.ComponentModel.DataAnnotations.Schema;
using TurnosApp.Core.Domain.Common;
using TurnosApp.Core.Domain.Enums;

namespace TurnosApp.Core.Domain.Entities;

// Tenant heredado vía Liquidacion, mismo criterio que TurnoServicio hereda vía Turno.
// La existencia de un LiquidacionDetalle para (TurnoId, ServicioId) cuya Liquidacion no
// esté Anulada ES el marcador de "este turno/servicio ya fue liquidado" — no hace falta
// una columna aparte en TurnoServicio (se deriva, no se duplica).
public class LiquidacionDetalle : BaseEntity
{
    public int LiquidacionId { get; set; }
    public int TurnoId { get; set; }
    public int ServicioId { get; set; }

    // Snapshot al momento de liquidar — protege contra cambios futuros de precio/regla.
    public decimal PrecioBaseAplicado { get; set; }
    public TipoComision TipoComisionSnapshot { get; set; }
    public decimal ValorComisionSnapshot { get; set; }

    // ── Calculado, NO persistido — mismo patrón que Cobro.MontoComision ──────────
    [NotMapped]
    public decimal MontoComisionCalculado => TipoComisionSnapshot switch
    {
        TipoComision.Porcentaje => PrecioBaseAplicado * ValorComisionSnapshot / 100m,
        TipoComision.MontoFijo => ValorComisionSnapshot,
        _ => 0m
    };

    // Navegación
    public Liquidacion Liquidacion { get; set; } = null!;
    public Turno Turno { get; set; } = null!;
    public Servicio Servicio { get; set; } = null!;
}
