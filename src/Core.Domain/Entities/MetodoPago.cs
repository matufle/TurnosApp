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

    // Distingue lo único que se cuenta físicamente en un arqueo de caja de lo que solo se concilia después.
    public bool EsEfectivo { get; set; }

    // Navegación
    public ICollection<Cobro> Cobros { get; set; } = [];
}
