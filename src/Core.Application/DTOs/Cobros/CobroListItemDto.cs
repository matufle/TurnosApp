using System;
using System.Collections.Generic;
using System.Text;

namespace TurnosApp.Core.Application.DTOs.Cobros;

public record CobroListItemDto(
    int Id,
    int TurnoId,
    string ClienteNombreCompleto,
    string ServiciosResumen,
    DateTime FechaHoraTurno,
    int? MetodoPagoId,
    string NombreMetodoPagoSnapshot,
    string TipoModificadorSnapshot,
    decimal PorcentajeModificadorSnapshot,
    decimal PrecioBase,
    decimal MontoModificadorCliente,
    decimal PrecioFinal,
    // null cuando el caller no tiene el permiso VerGananciaNeta (ej. Recepcionista).
    decimal? MontoComision,
    decimal? GananciaNeta,
    DateTime CreadoEn
);
