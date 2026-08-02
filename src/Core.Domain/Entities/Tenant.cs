using System;
using System.Collections.Generic;
using System.Text;

namespace TurnosApp.Core.Domain.Entities;

using TurnosApp.Core.Domain.Common;
using TurnosApp.Core.Domain.Enums;

public class Tenant : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;        // ej: "clinica-del-sur"
    public bool PermiteSolapamiento { get; set; } = false;
    public bool Activo { get; set; } = true;

    public string ColorPrimario { get; set; } = "#0EA5E9";
    public bool PermiteReservasPublicas { get; set; } = false;
    public DateTime FechaAlta { get; set; } = DateTime.UtcNow;
    public FrecuenciaLiquidacion FrecuenciaLiquidacion { get; set; } = FrecuenciaLiquidacion.Mensual;

    // Suscripción (Mercado Pago Preapproval). EsGrandfathered = true para todo tenant creado
    // antes del deploy de este gating (ver migración de backfill) — nunca pasa por el chequeo
    // de RequiereSuscripcionActivaAttribute, sin importar EstadoSuscripcion. A diferencia de
    // Stripe, MP no tiene un "Customer" separado: el preapproval se crea directo con el email
    // del pagador, así que solo hay un id externo que trackear (el del preapproval en sí).
    public int? PlanId { get; set; }
    public Plan? Plan { get; set; }
    public string? MercadoPagoPreapprovalId { get; set; }
    public EstadoSuscripcion EstadoSuscripcion { get; set; } = EstadoSuscripcion.Trial;
    public DateTime? SuscripcionVenceEn { get; set; }
    public bool EsGrandfathered { get; set; } = false;

    // Fecha en que el tenant entró en PastDue (falló el cobro recurrente). Se usa para calcular
    // el período de gracia (ver SuscripcionConstantes.DiasGraciaPastDue) antes de bloquear el
    // acceso — se limpia al reactivarse (Activa) o al cancelar explícitamente (Cancelada, que
    // no tiene gracia). Se setea una sola vez por episodio de PastDue, no en cada webhook.
    public DateTime? PastDueDesde { get; set; }

    // Navegación
    public ICollection<Recurso> Recursos { get; set; } = [];
    public ICollection<Cliente> Clientes { get; set; } = [];
    public ICollection<Servicio> Servicios { get; set; } = [];
    public ICollection<Turno> Turnos { get; set; } = [];
    public ICollection<MetodoPago> MetodosPago { get; set; } = [];
    public ICollection<Cobro> Cobros { get; set; } = [];
    public ICollection<SesionCaja> SesionesCaja { get; set; } = [];
    public ICollection<MovimientoCaja> MovimientosCaja { get; set; } = [];
    public ICollection<ReglaComision> ReglasComision { get; set; } = [];
    public ICollection<Liquidacion> Liquidaciones { get; set; } = [];
    public ICollection<AdelantoProfesional> AdelantosProfesional { get; set; } = [];
}