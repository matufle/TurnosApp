// src/config/runtimeConfig.ts (archivo nuevo)

// Le decimos a TypeScript que puede existir esta variable global,
// inyectada por env-config.js antes de que cargue el bundle de React.
declare global {
  interface Window {
    _env_?: {
      VITE_API_BASE_URL?: string;
    };
  }
}

// Prioridad: primero la config de runtime (Docker/producción),
// después la de build time (desarrollo local con npm run dev).
export const API_BASE_URL =
  window._env_?.VITE_API_BASE_URL || import.meta.env.VITE_API_BASE_URL || 'http://localhost:8080/api';