using System;
using System.Collections.Generic;
using System.Text;

namespace TurnosApp.Core.Domain.Entities;

using TurnosApp.Core.Domain.Common;
using TurnosApp.Core.Domain.Enums;

public class MetodoPago : TenantEntity
{
    public string Nombre { get; set; } = string.Empty;
    public TipoModificadorPago TipoModificador { get; set; } = TipoModificadorPago.Ninguno;

    // Siempre positivo — el signo del efecto sobre el cliente lo da TipoModificador.
    public decimal PorcentajeModificador { get; set; }

    // Siempre positivo — costo real que le cobra el proveedor de pago al negocio.
    // No afecta lo que paga el cliente, solo la ganancia neta.
    public decimal PorcentajeComision { get; set; }

    public bool Activo { get; set; } = true;

    // Navegación
    public ICollection<Cobro> Cobros { get; set; } = [];
}
