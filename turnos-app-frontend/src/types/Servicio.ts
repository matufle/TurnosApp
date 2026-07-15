// src/types/Servicio.ts
export interface Servicio {
  id: number;
  nombre: string;
  descripcion: string;
  duracionMinutos: number;
  precio: number;
}

export interface ServicioCreateDTO {
  nombre: string;
  descripcion: string;
  duracionMinutos: number;
  precio: number;
  
}

export interface ServicioUpdateDTO {
  nombre: string;
  descripcion: string;
  duracionMinutos: number;
  precio: number;
  
}