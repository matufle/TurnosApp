import type { Permiso } from './Permiso';

export interface Rol {
  id: number;
  nombre: string;
  esSistema: boolean;
  permisos: Permiso[];
}

export interface RolCreateDTO {
  nombre: string;
  permisos: Permiso[];
}

export type RolUpdateDTO = RolCreateDTO;
