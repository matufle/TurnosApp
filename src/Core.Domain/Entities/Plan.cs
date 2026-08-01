namespace TurnosApp.Core.Domain.Entities;

using TurnosApp.Core.Domain.Common;

// Catálogo global (no tenant-scoped, como MetodoPago): un solo plan flat por ahora,
// modelado como tabla en vez de hardcodeado para poder agregar tiers después sin romper
// el esquema. Sin un id externo cacheado (a diferencia de lo que hubiera sido un
// StripePriceId): la API de Preapproval "sin plan asociado" de Mercado Pago recibe
// PrecioMensual/moneda/frecuencia inline en cada alta, no requiere crear un recurso
// de catálogo del lado de MP primero.
public class Plan : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public decimal PrecioMensual { get; set; }
    public int TrialDias { get; set; } = 30;
    public bool Activo { get; set; } = true;

    public ICollection<Tenant> Tenants { get; set; } = [];
}
