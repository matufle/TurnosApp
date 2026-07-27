export interface RecursoPublico {
  id: number;
  nombre: string;
  descripcion: string | null;
  colorHex: string;
}

export interface CrearTurnoPublicoRequest {
  recursoId: number;
  servicioIds: number[];
  fechaHoraInicio: string;
}
