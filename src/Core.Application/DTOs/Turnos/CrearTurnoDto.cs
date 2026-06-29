using System;
using System.Collections.Generic;
using System.Text;
namespace TurnosApp.Core.Application.DTOs.Turnos;

/// <summary>
/// DTO de entrada para reservar un turno.
/// FechaHoraFin no se recibe: se calcula a partir de
/// FechaHoraInicio + DuracionMinutos del Servicio.
/// TenantId no se recibe: lo resuelve ITenantProvider en Application.
/// </summary>
public record CrearTurnoDto(
    int ClienteId,
    int RecursoId,
    int ServicioId,
    DateTime FechaHoraInicio
);