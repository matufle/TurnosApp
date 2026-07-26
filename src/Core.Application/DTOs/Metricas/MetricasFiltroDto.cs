using System;

namespace TurnosApp.Core.Application.DTOs.Metricas;

// Filtro compartido por los 5 endpoints de Métricas; cada AppService ignora
// los campos que no le aplican a su pestaña (todos son opcionales).
public record MetricasFiltroDto(
    DateTime? FechaDesde,
    DateTime? FechaHasta,
    int? RecursoId,
    int? ServicioId,
    int? MetodoPagoId,
    string? Estado
);
