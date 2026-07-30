// src/api/liquidacionesService.ts
import httpClient from './httpClient';
import type {
  ReglaComision,
  ReglaComisionCreateDTO,
  ReglaComisionUpdateDTO,
  AdelantoProfesional,
  AdelantoProfesionalCreateDTO,
  Liquidacion,
  LiquidacionListItem,
  MarcarPagadaLiquidacionDTO,
  AnularLiquidacionDTO,
} from '../types/Liquidacion';

export const reglasComisionService = {
  getByRecurso: async (recursoId: number): Promise<ReglaComision[]> => {
    const response = await httpClient.get<ReglaComision[]>('/reglascomision', { params: { recursoId } });
    return response.data;
  },

  create: async (dto: ReglaComisionCreateDTO): Promise<ReglaComision> => {
    const response = await httpClient.post<ReglaComision>('/reglascomision', dto);
    return response.data;
  },

  update: async (id: number, dto: ReglaComisionUpdateDTO): Promise<ReglaComision> => {
    const response = await httpClient.put<ReglaComision>(`/reglascomision/${id}`, dto);
    return response.data;
  },
};

export const adelantosProfesionalService = {
  getByRecurso: async (recursoId: number): Promise<AdelantoProfesional[]> => {
    const response = await httpClient.get<AdelantoProfesional[]>('/adelantosprofesional', { params: { recursoId } });
    return response.data;
  },

  create: async (dto: AdelantoProfesionalCreateDTO): Promise<AdelantoProfesional> => {
    const response = await httpClient.post<AdelantoProfesional>('/adelantosprofesional', dto);
    return response.data;
  },
};

export const liquidacionesService = {
  getAll: async (): Promise<LiquidacionListItem[]> => {
    const response = await httpClient.get<LiquidacionListItem[]>('/liquidaciones');
    return response.data;
  },

  getMias: async (): Promise<LiquidacionListItem[]> => {
    const response = await httpClient.get<LiquidacionListItem[]>('/liquidaciones/mias');
    return response.data;
  },

  getById: async (id: number): Promise<Liquidacion> => {
    const response = await httpClient.get<Liquidacion>(`/liquidaciones/${id}`);
    return response.data;
  },

  marcarPagada: async (id: number, dto: MarcarPagadaLiquidacionDTO): Promise<Liquidacion> => {
    const response = await httpClient.patch<Liquidacion>(`/liquidaciones/${id}/marcar-pagada`, dto);
    return response.data;
  },

  anular: async (id: number, dto: AnularLiquidacionDTO): Promise<Liquidacion> => {
    const response = await httpClient.patch<Liquidacion>(`/liquidaciones/${id}/anular`, dto);
    return response.data;
  },

  generar: async (): Promise<void> => {
    await httpClient.post('/liquidaciones/generar');
  },
};
