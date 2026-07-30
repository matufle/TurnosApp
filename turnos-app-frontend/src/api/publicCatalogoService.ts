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
  // Query string armada a mano (en vez de dejar que axios serialice el array servicioIds):
  // ASP.NET Core espera "servicioIds=1&servicioIds=2" (clave repetida, sin corchetes) para
  // bindear un [FromQuery] int[] — así no depende de la serialización por default de axios.
  getDisponibilidad: async (slug: string, recursoId: number, servicioIds: number[], fecha: string): Promise<string[]> => {
    const params = new URLSearchParams();
    params.set('recursoId', String(recursoId));
    servicioIds.forEach((id) => params.append('servicioIds', String(id)));
    params.set('fecha', fecha);

    const response = await clienteHttpClient.get<string[]>(`/public/tenants/${slug}/disponibilidad?${params.toString()}`);
    return response.data;
  },
};
