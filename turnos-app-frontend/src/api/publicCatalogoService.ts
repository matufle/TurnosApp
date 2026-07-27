import clienteHttpClient from './clienteHttpClient';
import type { Servicio } from '../types/Servicio';
import type { RecursoPublico } from '../types/ReservaPublica';

export const publicCatalogoService = {
  getServicios: async (slug: string): Promise<Servicio[]> => {
    const response = await clienteHttpClient.get<Servicio[]>(`/public/tenants/${slug}/servicios`);
    return response.data;
  },

  getRecursos: async (slug: string): Promise<RecursoPublico[]> => {
    const response = await clienteHttpClient.get<RecursoPublico[]>(`/public/tenants/${slug}/recursos`);
    return response.data;
  },

  // fecha: "YYYY-MM-DD" (fecha local, ver combinarFechaYHora en TurnosPage.tsx).
  // Devuelve horas UTC-equivalentes tal cual las manda el backend — ver horarioTimezone.ts.
  getDisponibilidad: async (slug: string, recursoId: number, servicioId: number, fecha: string): Promise<string[]> => {
    const response = await clienteHttpClient.get<string[]>(`/public/tenants/${slug}/disponibilidad`, {
      params: { recursoId, servicioId, fecha },
    });
    return response.data;
  },
};
