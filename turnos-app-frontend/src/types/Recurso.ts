export interface Recurso {
  id: number;
  nombre: string;
  descripcion: string;
  colorHex: string;
  activo: boolean;
  usuarioId: number | null;
  usuarioNombre: string | null;
}

export interface RecursoCreateDTO {
  nombre: string;
  descripcion: string;
  colorHex: string;
  usuarioId: number | null;
}

export interface RecursoUpdateDTO extends RecursoCreateDTO {
  activo: boolean;
}

export interface UsuarioParaVincular {
  id: number;
  nombre: string;
}
