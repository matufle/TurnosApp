export interface TenantPublico {
  tenantId: number;
  nombre: string;
  slug: string;
  colorPrimario: string;
}

export interface ClienteRegistroRequest {
  tenantSlug: string;
  nombre: string;
  apellido: string;
  email: string;
  telefono?: string;
  password: string;
}

export interface ClienteLoginRequest {
  tenantSlug: string;
  email: string;
  password: string;
  recordarMe?: boolean;
}

export interface ClienteAuthResponse {
  token: string;
  tenantId: number;
  email: string;
}

export interface ClienteMeResponse {
  clienteId: number;
  nombre: string;
  apellido: string;
  email: string;
  tenantId: number;
}
