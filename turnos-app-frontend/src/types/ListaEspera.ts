// src/types/ListaEspera.ts
export type EstadoListaEspera = 'Activa' | 'Notificada' | 'Cancelada';

export interface ListaEsperaEntry {
  id: number;
  clienteId: number;
  clienteNombreCompleto: string;
  recursoId: number;
  recursoNombre: string;
  servicioId: number | null;
  servicioNombre: string | null;
  fechaDesde: string;
  fechaHasta: string;
  estado: EstadoListaEspera;
  notificadoEn: string | null;
}

export interface CrearListaEsperaDTO {
  clienteId: number;
  recursoId: number;
  servicioId: number | null;
  fechaDesde: string;
  fechaHasta: string;
}
