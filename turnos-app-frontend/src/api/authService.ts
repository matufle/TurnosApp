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

export interface RegistroPendienteResponse {
  email: string;
}

export interface RegisterRequest {
  nombreNegocio: string;
  email: string;
  password: string;
  turnstileToken: string;
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
  onboardingCompletado: boolean;
}

export const authService = {
  login: async (credentials: LoginRequest): Promise<LoginResponse> => {
    const response = await httpClient.post<LoginResponse>('/auth/login', credentials);
    return response.data;
  },

register: async (dto: RegisterRequest): Promise<RegistroPendienteResponse> => {
    const response = await httpClient.post<RegistroPendienteResponse>('/auth/register', dto);
    return response.data;
  },

  me: async (): Promise<MeResponse> => {
    const response = await httpClient.get<MeResponse>('/auth/me');
    return response.data;
  },

  completarOnboarding: async (): Promise<void> => {
    await httpClient.patch('/auth/onboarding');
  },

  confirmarEmail: async (token: string): Promise<void> => {
    await httpClient.post('/auth/confirmar-email', { token });
  },

  reenviarConfirmacion: async (email: string): Promise<void> => {
    await httpClient.post('/auth/reenviar-confirmacion', { email });
  },

  olvidePassword: async (email: string): Promise<void> => {
    await httpClient.post('/auth/olvide-password', { email });
  },

  resetPassword: async (token: string, nuevaPassword: string): Promise<void> => {
    await httpClient.post('/auth/reset-password', { token, nuevaPassword });
  },
};