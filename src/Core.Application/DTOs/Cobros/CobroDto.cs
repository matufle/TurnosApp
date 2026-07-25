using System;
using System.Collections.Generic;
using System.Text;

namespace TurnosApp.Core.Application.DTOs.Cobros;

public record CobroDto(
    int Id,
    int TurnoId,
    int? MetodoPagoId,
    string NombreMetodoPagoSnapshot,
    string TipoModificadorSnapshot,
    decimal PorcentajeModificadorSnapshot,
    decimal PorcentajeComisionSnapshot,
    decimal PrecioBase,
    decimal MontoModificadorCliente,
    decimal PrecioFinal,
    decimal MontoComision,
    decimal GananciaNeta,
    DateTime CreadoEn,
    string? CreadoPor,
    DateTime? ModificadoEn,
    string? ModificadoPor
);
