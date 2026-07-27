import httpClient from './httpClient';
import type { HorarioAtencion, HorarioAtencionItem } from '../types/HorarioAtencion';

export const horariosAtencionService = {
  getByRecurso: async (recursoId: number): Promise<HorarioAtencion[]> => {
    const response = await httpClient.get<HorarioAtencion[]>(`/recursos/${recursoId}/horarios`);
    return response.data;
  },

  reemplazar: async (recursoId: number, horarios: HorarioAtencionItem[]): Promise<HorarioAtencion[]> => {
    const response = await httpClient.put<HorarioAtencion[]>(`/recursos/${recursoId}/horarios`, { horarios });
    return response.data;
  },
};
