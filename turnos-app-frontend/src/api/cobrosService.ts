// src/api/cobrosService.ts
import httpClient from './httpClient';
import type { Cobro, CobroCreateDTO, CobroUpdateDTO, HistorialCobros, HistorialCobrosFiltro } from '../types/Cobro';

export const cobrosService = {
  getByTurno: async (turnoId: number): Promise<Cobro[]> => {
    const response = await httpClient.get<Cobro[]>('/cobros', { params: { turnoId } });
    return response.data;
  },

  crear: async (dto: CobroCreateDTO): Promise<Cobro> => {
    const response = await httpClient.post<Cobro>('/cobros', dto);
    return response.data;
  },

  actualizar: async (id: number, dto: CobroUpdateDTO): Promise<Cobro> => {
    const response = await httpClient.put<Cobro>(`/cobros/${id}`, dto);
    return response.data;
  },

  getHistorial: async (filtro: HistorialCobrosFiltro): Promise<HistorialCobros> => {
    const response = await httpClient.get<HistorialCobros>('/cobros/historial', { params: filtro });
    return response.data;
  },
};
