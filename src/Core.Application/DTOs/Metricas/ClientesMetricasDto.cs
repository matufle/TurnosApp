using System.Collections.Generic;

namespace TurnosApp.Core.Application.DTOs.Metricas;

// Cliente no tiene una fecha de alta propia (ver Core.Domain.Entities.Cliente):
// se usa MIN(Turno.FechaHoraInicio) por cliente como proxy de "alta" y
// MAX(Turno.FechaHoraInicio) como última actividad (para el corte de inactividad de 60 días).
public record ClientesMetricasDto(
    int ClientesNuevos,
    decimal PorcentajeRecurrentes,
    int ClientesInactivos,
    IReadOnlyList<PuntoSerieDto> NuevosPorMes,
    IReadOnlyList<DistribucionDto> NuevosRecurrentesInactivos,
    IReadOnlyList<RankingItemDto> TopClientesPorFacturacion
);
