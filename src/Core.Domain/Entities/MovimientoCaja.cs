using TurnosApp.Core.Domain.Common;
using TurnosApp.Core.Domain.Enums;

namespace TurnosApp.Core.Domain.Entities;

// Inmutable una vez creado (sin Update en el repositorio) — para corregir un error se
// carga un movimiento de reversa (MovimientoOrigenId), igual que un Turno no se re-edita.
public class MovimientoCaja : TenantEntity
{
    public int SesionCajaId { get; set; }

    public TipoMovimientoCaja Tipo { get; set; }

    // Siempre positivo — el signo lo da Tipo, mismo criterio que MetodoPago.PorcentajeModificador.
    public decimal Monto { get; set; }

    public int? MetodoPagoId { get; set; }   // nullable: el movimiento sobrevive si el método se hard-deletea

    // Snapshot histórico tomado del MetodoPago al registrar el movimiento — si el método
    // se reconfigura (o deja de ser "efectivo") después, no debe alterar cierres ya calculados.
    public string NombreMetodoPagoSnapshot { get; set; } = string.Empty;
    public bool EsEfectivoSnapshot { get; set; }

    public string Concepto { get; set; } = string.Empty;
    public DateTime FechaHora { get; set; }

    public int UsuarioId { get; set; }

    // Set solo en movimientos automáticos originados por un Cobro (ver CajaAppService.SincronizarMovimientoDeCobroAsync).
    public int? CobroId { get; set; }

    // Set solo en movimientos de reversa — apunta al movimiento que reversan.
    public int? MovimientoOrigenId { get; set; }

    // Navegación
    public SesionCaja SesionCaja { get; set; } = null!;
    public MetodoPago? MetodoPago { get; set; }
    public Usuario Usuario { get; set; } = null!;
    public Cobro? Cobro { get; set; }
    public MovimientoCaja? MovimientoOrigen { get; set; }
}
