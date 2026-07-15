export interface Recurso {
  id: number;
  nombre: string;
  descripcion: string;
  colorHex: string;
}

export interface RecursoCreateDTO {
  nombre: string;
  descripcion: string;
  colorHex: string;
}