export interface Recurso {
  id: number;
  nombre: string;
  descripcion: string;
}

export interface RecursoCreateDTO {
  nombre: string;
  descripcion: string;
}