using System;
using System.Collections.Generic;
using System.Text;
using TurnosApp.Core.Domain.Enums;

namespace TurnosApp.Core.Application.DTOs.Turnos;

/// <summary>
/// DTO de salida. Incluye FechaHoraFin calculada y el estado actual del turno.
/// Expone un único ServicioId para simplificar mientras el caso de uso
/// soporte un servicio por turno.
/// </summary>
public record TurnoDto(
    int Id,
    int ClienteId,
    int RecursoId,
    int ServicioId,
    DateTime FechaHoraInicio,
    DateTime FechaHoraFin,
    EstadoTurno Estado
);