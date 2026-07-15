// src/types/Cliente.ts
export interface Cliente {
  id: number;
  nombre: string;
  apellido: string;
  email: string | null;
  telefono: string | null;
  notasAdicionales: string | null;
}

export interface CreateClienteDTO {
  nombre: string;
  apellido: string;
  email?: string;
  telefono?: string;
  notasAdicionales?: string;
}

export interface UpdateClienteDTO {
  nombre: string;
  apellido: string;
  email?: string;
  telefono?: string;
  notasAdicionales?: string;
}