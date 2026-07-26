// src/types/Metricas.ts
// Espejo de src/Core.Application/DTOs/Metricas/*.cs

export interface MetricasFiltro {
  fechaDesde?: string;
  fechaHasta?: string;
  recursoId?: number;
  servicioId?: number;
  metodoPagoId?: number;
  estado?: string;
}

export interface PuntoSerie {
  etiqueta: string;
  valor: number;
}

export interface SerieMultiple {
  etiqueta: string;
  ingresos: number;
  // null cuando el usuario no tiene el permiso VerGananciaNeta.
  comision: number | null;
  gananciaNeta: number | null;
}

export interface Distribucion {
  categoria: string;
  cantidad: number;
  porcentaje: number;
}

export interface RankingItem {
  id: number;
  nombre: string;
  valor: number;
  cantidad: number;
}

export interface HeatmapCelda {
  diaSemana: number; // 0=Domingo..6=Sábado
  hora: number; // 0-23
  cantidad: number;
}

export interface ResumenMetricas {
  ingresosPeriodo: number;
  turnosCompletados: number;
  tasaCancelacion: number;
  saldoPendientePeriodo: number;
  ingresosPorDia: PuntoSerie[];
  turnosPorEstado: Distribucion[];
  topServiciosPorIngresos: RankingItem[];
  topRecursosPorTurnos: RankingItem[];
}

export interface IngresosMetricas {
  ingresosTotales: number;
  gananciaNeta: number | null;
  ticketPromedio: number;
  ingresosComisionGanancia: SerieMultiple[];
  ingresosPorMetodoPago: RankingItem[];
  estadoPagoTurnos: Distribucion[];
}

export interface TurnosMetricas {
  turnosTotales: number;
  tasaCancelacion: number;
  tasaAusentismo: number;
  anticipacionPromedioHoras: number;
  heatmap: HeatmapCelda[];
  creados: PuntoSerie[];
  completados: PuntoSerie[];
  ocupacionPorRecurso: RankingItem[];
}

export interface ClientesMetricas {
  clientesNuevos: number;
  porcentajeRecurrentes: number;
  clientesInactivos: number;
  nuevosPorMes: PuntoSerie[];
  nuevosRecurrentesInactivos: Distribucion[];
  topClientesPorFacturacion: RankingItem[];
}

export interface ServiciosRecursosMetricas {
  serviciosMasReservados: RankingItem[];
  serviciosMasRentables: RankingItem[];
  serviciosBajaDemanda: RankingItem[];
  facturacionPorRecurso: RankingItem[];
  completadosPorRecurso: RankingItem[];
  canceladosPorRecurso: RankingItem[];
}
