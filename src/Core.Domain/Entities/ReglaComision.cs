using TurnosApp.Core.Domain.Common;
using TurnosApp.Core.Domain.Enums;

namespace TurnosApp.Core.Domain.Entities;

// Configuración de cuánto cobra un profesional por lo que atiende. ServicioId null = regla
// base del Recurso (aplica a cualquier servicio sin override puntual); con valor = excepción
// para esa combinación Recurso+Servicio específica (resuelta con prioridad sobre la base).
public class ReglaComision : TenantEntity
{
    public int RecursoId { get; set; }
    public int? ServicioId { get; set; }

    public TipoComision Tipo { get; set; }

    // Porcentaje (0-100) o monto fijo en $, según Tipo.
    public decimal Valor { get; set; }

    public bool Activo { get; set; } = true;

    // Navegación
    public Recurso Recurso { get; set; } = null!;
    public Servicio? Servicio { get; set; }
}
