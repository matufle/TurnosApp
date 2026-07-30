using System.ComponentModel.DataAnnotations.Schema;
using TurnosApp.Core.Domain.Common;
using TurnosApp.Core.Domain.Enums;

namespace TurnosApp.Core.Domain.Entities;

public class Liquidacion : TenantEntity
{
    public int RecursoId { get; set; }

    public DateTime PeriodoDesde { get; set; }
    public DateTime PeriodoHasta { get; set; }
    public DateTime FechaGeneracion { get; set; } = DateTime.UtcNow;

    public EstadoLiquidacion Estado { get; set; } = EstadoLiquidacion.Generada;

    public DateTime? FechaPago { get; set; }
    public int? UsuarioPagoId { get; set; }

    public string? Observaciones { get; set; }

    // ── Calculado, NO persistido — mismo patrón que SesionCaja.MontoEsperadoEfectivo ──
    [NotMapped]
    public decimal MontoBrutoComision => Detalles.Sum(d => d.MontoComisionCalculado);

    [NotMapped]
    public decimal MontoAdelantos => Adelantos.Sum(a => a.Monto);

    [NotMapped]
    public decimal MontoNeto => MontoBrutoComision - MontoAdelantos;

    // Navegación
    public Recurso Recurso { get; set; } = null!;
    public Usuario? UsuarioPago { get; set; }
    public ICollection<LiquidacionDetalle> Detalles { get; set; } = [];
    public ICollection<AdelantoProfesional> Adelantos { get; set; } = [];
}
