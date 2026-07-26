// src/api/metricasService.ts
import httpClient from './httpClient';
import type {
  MetricasFiltro,
  ResumenMetricas,
  IngresosMetricas,
  TurnosMetricas,
  ClientesMetricas,
  ServiciosRecursosMetricas,
} from '../types/Metricas';

export const metricasService = {
  getResumen: async (filtro: MetricasFiltro): Promise<ResumenMetricas> => {
    const response = await httpClient.get<ResumenMetricas>('/metricas/resumen', { params: filtro });
    return response.data;
  },

  getIngresos: async (filtro: MetricasFiltro): Promise<IngresosMetricas> => {
    const response = await httpClient.get<IngresosMetricas>('/metricas/ingresos', { params: filtro });
    return response.data;
  },

  getTurnos: async (filtro: MetricasFiltro): Promise<TurnosMetricas> => {
    const response = await httpClient.get<TurnosMetricas>('/metricas/turnos', { params: filtro });
    return response.data;
  },

  getClientes: async (filtro: MetricasFiltro): Promise<ClientesMetricas> => {
    const response = await httpClient.get<ClientesMetricas>('/metricas/clientes', { params: filtro });
    return response.data;
  },

  getServiciosRecursos: async (filtro: MetricasFiltro): Promise<ServiciosRecursosMetricas> => {
    const response = await httpClient.get<ServiciosRecursosMetricas>('/metricas/servicios-recursos', { params: filtro });
    return response.data;
  },
};
