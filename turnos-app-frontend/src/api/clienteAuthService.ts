import clienteHttpClient from './clienteHttpClient';
import type {
  ClienteAuthResponse,
  ClienteLoginRequest,
  ClienteMeResponse,
  ClienteRegistroRequest,
} from '../types/ClienteAuth';

export const clienteAuthService = {
  registrar: async (dto: ClienteRegistroRequest): Promise<ClienteAuthResponse> => {
    const response = await clienteHttpClient.post<ClienteAuthResponse>('/cliente-auth/registro', dto);
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
};
