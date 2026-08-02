import clienteHttpClient from './clienteHttpClient';
import type {
  ClienteAuthResponse,
  ClienteLoginRequest,
  ClienteMeResponse,
  ClienteRegistroPendienteResponse,
  ClienteRegistroRequest,
} from '../types/ClienteAuth';

export const clienteAuthService = {
  registrar: async (dto: ClienteRegistroRequest): Promise<ClienteRegistroPendienteResponse> => {
    const response = await clienteHttpClient.post<ClienteRegistroPendienteResponse>('/cliente-auth/registro', dto);
    return response.data;
  },

  login: async (dto: ClienteLoginRequest): Promise<ClienteAuthResponse> => {
    const response = await clienteHttpClient.post<ClienteAuthResponse>('/cliente-auth/login', dto);
    return response.data;
  },

  me: async (): Promise<ClienteMeResponse> => {
    const response = await clienteHttpClient.get<ClienteMeResponse>('/cliente-auth/me');
    return response.data;
  },

  confirmarEmail: async (tenantSlug: string, token: string): Promise<void> => {
    await clienteHttpClient.post('/cliente-auth/confirmar-email', { tenantSlug, token });
  },

  reenviarConfirmacion: async (tenantSlug: string, email: string): Promise<void> => {
    await clienteHttpClient.post('/cliente-auth/reenviar-confirmacion', { tenantSlug, email });
  },

  olvidePassword: async (tenantSlug: string, email: string): Promise<void> => {
    await clienteHttpClient.post('/cliente-auth/olvide-password', { tenantSlug, email });
  },

  resetPassword: async (tenantSlug: string, token: string, nuevaPassword: string): Promise<void> => {
    await clienteHttpClient.post('/cliente-auth/reset-password', { tenantSlug, token, nuevaPassword });
  },
};
