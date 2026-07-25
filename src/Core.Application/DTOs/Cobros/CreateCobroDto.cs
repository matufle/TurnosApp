using System;
using System.Collections.Generic;
using System.Text;

namespace TurnosApp.Core.Application.DTOs.Cobros;

public record CreateCobroDto(
    int TurnoId,
    int MetodoPagoId,
    decimal PrecioBase
);
