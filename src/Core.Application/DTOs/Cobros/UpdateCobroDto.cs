using System;
using System.Collections.Generic;
using System.Text;

namespace TurnosApp.Core.Application.DTOs.Cobros;

// TurnoId no viaja: un cobro no cambia de turno, solo su método y monto.
public record UpdateCobroDto(
    int MetodoPagoId,
    decimal PrecioBase
);
