using System;
using System.Collections.Generic;
using System.Text;

namespace TurnosApp.Core.Application.DTOs.MetodosPago;

public record CreateMetodoPagoDto(
    string Nombre,
    string TipoModificador,
    decimal PorcentajeModificador,
    decimal PorcentajeComision
);
