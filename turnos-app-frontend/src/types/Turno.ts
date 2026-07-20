// src/types/Turno.ts
export interface Turno {
  id: number;
  recursoId: number;
  recursoNombre: string;
  clienteId: number;
  clienteNombreCompleto: string;
  fechaHoraInicio: string;
  fechaHoraFin: string;
  estado: string;
  servicios: string[];
  precioTotal: number;
}

export interface ClienteInlineDTO {
  nombre: string;
  apellido: string;
  telefono?: string;
}

export interface CrearTurnoDTO {
  clienteId: number | null;
  clienteNuevo: ClienteInlineDTO | null;
  recursoId: number;
  servicioIds: number[];
  fechaHoraInicio: string;
}

// Coincide con el enum EstadoTurno del backend (src/Core.Domain/Enums/EstadoTurno.cs),
// sin incluir "Cancelado": para cancelar existe la acción dedicada de cancelar.
export type EstadoTurnoEditable = 'Pendiente' | 'Confirmado' | 'EnCurso' | 'Completado' | 'Ausente';

export interface CambiarEstadoTurnoDTO {
  nuevoEstado: EstadoTurnoEditable;
}