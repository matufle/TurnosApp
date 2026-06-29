using System;
using System.Collections.Generic;
using System.Text;

namespace TurnosApp.Core.Domain.Entities;

using TurnosApp.Core.Domain.Common;

/// <summary>
/// Tabla de unión explícita entre Turno y Servicio.
/// Usar tabla explícita (en vez de M:N implícita de EF) permite
/// agregar propiedades futuras como orden, precio aplicado en el momento, etc.
/// </summary>
public class TurnoServicio : BaseEntity
{
    public int TurnoId { get; set; }
    public int ServicioId { get; set; }
    public int Orden { get; set; }                             // posición secuencial del servicio

    // Precio snapshot al momento de reservar (protege contra cambios futuros de precio)
    public decimal PrecioAplicado { get; set; }

    // Navegación
    public Turno Turno { get; set; } = null!;
    public Servicio Servicio { get; set; } = null!;
}