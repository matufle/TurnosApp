using System.ComponentModel.DataAnnotations.Schema;
using TurnosApp.Core.Domain.Common;
using TurnosApp.Core.Domain.Enums;

namespace TurnosApp.Core.Domain.Entities;

public class SesionCaja : TenantEntity
{
    public int UsuarioAperturaId { get; set; }
    public int? UsuarioCierreId { get; set; }

    public DateTime FechaApertura { get; set; }
    public DateTime? FechaCierre { get; set; }

    // Fondo fijo declarado al abrir — solo efectivo, es lo único que se cuenta físicamente.
    public decimal MontoInicial { get; set; }

    // Efectivo contado por el usuario al cerrar.
    public decimal? MontoFinalDeclarado { get; set; }

    public EstadoSesionCaja Estado { get; set; } = EstadoSesionCaja.Abierta;

    // true si se cerró vía Permiso.ForzarCierreCaja (UsuarioCierreId != UsuarioAperturaId).
    public bool CierreForzado { get; set; }

    public string? Observaciones { get; set; }

    // ── Calculado, NO persistido — mismo patrón que Cobro.PrecioFinal ──────────
    // El arqueo (esperado/diferencia) es SOLO de efectivo: es lo único que se cuenta
    // físicamente. El resto de los medios de pago se concilian aparte, no se "cuentan".
    [NotMapped]
    public decimal MontoEsperadoEfectivo =>
        MontoInicial
        + Movimientos.Where(m => m.EsEfectivoSnapshot && m.Tipo == TipoMovimientoCaja.Ingreso).Sum(m => m.Monto)
        - Movimientos.Where(m => m.EsEfectivoSnapshot && m.Tipo == TipoMovimientoCaja.Egreso).Sum(m => m.Monto);

    [NotMapped]
    public decimal? Diferencia => MontoFinalDeclarado is null ? null : MontoFinalDeclarado - MontoEsperadoEfectivo;

    // Navegación
    public Usuario UsuarioApertura { get; set; } = null!;
    public Usuario? UsuarioCierre { get; set; }
    public ICollection<MovimientoCaja> Movimientos { get; set; } = [];
}
