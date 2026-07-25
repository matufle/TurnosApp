import httpClient from './httpClient';

export interface LoginRequest {
  email: string;
  password: string;
  recordarme?: boolean;
}

export interface LoginResponse {
  token: string;
  tenantId: number;
  email: string;
}

export interface MeResponse {
  usuarioId: number;
  nombre: string;
  email: string;
  tenantId: number;
  rolId: number;
  rolNombre: string;
  permisos: string[];
  recursoId: number | null;
}

export const authService = {
  login: async (credentials: LoginRequest): Promise<LoginResponse> => {
    const response = await httpClient.post<LoginResponse>('/auth/login', credentials);
    return response.data;
  },

register: async (credentials: LoginRequest): Promise<LoginResponse> => {
    const response = await httpClient.post<LoginResponse>('/auth/register', credentials);
    return response.data;
  },

  me: async (): Promise<MeResponse> => {
    const response = await httpClient.get<MeResponse>('/auth/me');
    return response.data;
  },
};