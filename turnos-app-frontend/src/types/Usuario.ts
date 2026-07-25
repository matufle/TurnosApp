export interface Usuario {
  id: number;
  nombre: string;
  email: string;
  rolId: number;
  rolNombre: string;
  activo: boolean;
}

export interface UsuarioCreateDTO {
  nombre: string;
  email: string;
  password: string;
  rolId: number;
}

export interface UsuarioUpdateDTO {
  nombre: string;
  rolId: number;
}
