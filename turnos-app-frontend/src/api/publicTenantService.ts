import clienteHttpClient from './clienteHttpClient';
import type { TenantPublico } from '../types/ClienteAuth';

export const publicTenantService = {
  getBySlug: async (slug: string): Promise<TenantPublico> => {
    const response = await clienteHttpClient.get<TenantPublico>(`/public/tenants/${slug}`);
    return response.data;
  },
};
