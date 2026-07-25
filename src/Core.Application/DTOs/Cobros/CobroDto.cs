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
    // null cuando el caller no tiene el permiso VerGananciaNeta (ej. Recepcionista):
    // necesita listar/crear cobros, pero no ver la utilidad neta del negocio.
    decimal? MontoComision,
    decimal? GananciaNeta,
    DateTime CreadoEn,
    string? CreadoPor,
    DateTime? ModificadoEn,
    string? ModificadoPor
);
