using System;
using System.Collections.Generic;
using System.Text;

namespace TurnosApp.Core.Domain.Entities;

using System.ComponentModel.DataAnnotations.Schema;
using TurnosApp.Core.Domain.Common;
using TurnosApp.Core.Domain.Enums;

public class Cobro : TenantEntity
{
    public int TurnoId { get; set; }
    public int? MetodoPagoId { get; set; }   // nullable: el cobro sobrevive si el método se hard-deletea

    // Snapshot histórico tomado del MetodoPago al crear/editar el cobro.
    // Editar el % de un MetodoPago a futuro NO debe afectar cobros ya guardados.
    public string NombreMetodoPagoSnapshot { get; set; } = string.Empty;
    public TipoModificadorPago TipoModificadorSnapshot { get; set; } = TipoModificadorPago.Ninguno;
    public decimal PorcentajeModificadorSnapshot { get; set; }
    public decimal PorcentajeComisionSnapshot { get; set; }

    // Porción de la deuda del turno que se salda con ESTE cobro puntual (permite pagos parciales/split).
    public decimal PrecioBase { get; set; }

    // ── Calculado, NO persistido — mismo patrón que Turno.FechaHoraFin ──────────
    [NotMapped]
    public decimal MontoModificadorCliente => TipoModificadorSnapshot switch
    {
        TipoModificadorPago.Bonificacion => -(PrecioBase * PorcentajeModificadorSnapshot / 100m),
        TipoModificadorPago.Recargo => PrecioBase * PorcentajeModificadorSnapshot / 100m,
        _ => 0m
    };

    [NotMapped]
    public decimal PrecioFinal => PrecioBase + MontoModificadorCliente;

    [NotMapped]
    public decimal MontoComision => PrecioFinal * PorcentajeComisionSnapshot / 100m;

    [NotMapped]
    public decimal GananciaNeta => PrecioFinal - MontoComision;

    // Auditoría (Cobro es editable, por eso incluye ModificadoPor a diferencia de Turno)
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
    public DateTime? ModificadoEn { get; set; }
    public string? CreadoPor { get; set; }
    public string? ModificadoPor { get; set; }

    // Navegación
    public Turno Turno { get; set; } = null!;
    public MetodoPago? MetodoPago { get; set; }
}
