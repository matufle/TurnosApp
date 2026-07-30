using System;
using System.Collections.Generic;
using System.Text;

namespace TurnosApp.Core.Application.DTOs.MetodosPago;

public record UpdateMetodoPagoDto(
    string Nombre,
    string TipoModificador,
    decimal PorcentajeModificador,
    decimal PorcentajeComision,
    bool Activo,
    bool EsEfectivo
);
