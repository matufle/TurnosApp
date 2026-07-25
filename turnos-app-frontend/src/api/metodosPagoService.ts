// src/api/metodosPagoService.ts
import httpClient from './httpClient';
import type { MetodoPago, MetodoPagoCreateDTO, MetodoPagoUpdateDTO } from '../types/MetodoPago';

export const metodosPagoService = {
  getAll: async (): Promise<MetodoPago[]> => {
    const response = await httpClient.get<MetodoPago[]>('/metodospago');
    return response.data;
  },

  create: async (dto: MetodoPagoCreateDTO): Promise<MetodoPago> => {
    const response = await httpClient.post<MetodoPago>('/metodospago', dto);
    return response.data;
  },

  update: async (id: number, dto: MetodoPagoUpdateDTO): Promise<MetodoPago> => {
    const response = await httpClient.put<MetodoPago>(`/metodospago/${id}`, dto);
    return response.data;
  },

  desactivar: async (id: number): Promise<MetodoPago> => {
    const response = await httpClient.patch<MetodoPago>(`/metodospago/${id}/desactivar`);
    return response.data;
  },
};
