// src/config/runtimeConfig.ts (archivo nuevo)

// Le decimos a TypeScript que puede existir esta variable global,
// inyectada por env-config.js antes de que cargue el bundle de React.
declare global {
  interface Window {
    _env_?: {
      VITE_API_BASE_URL?: string;
      VITE_SENTRY_DSN?: string;
      VITE_MIXPANEL_TOKEN?: string;
      VITE_HOTJAR_ID?: string;
    };
  }
}

// Prioridad: primero la config de runtime (Docker/producción),
// después la de build time (desarrollo local con npm run dev).
export const API_BASE_URL =
  window._env_?.VITE_API_BASE_URL || import.meta.env.VITE_API_BASE_URL || 'http://localhost:8080/api';

export const SENTRY_DSN = window._env_?.VITE_SENTRY_DSN || import.meta.env.VITE_SENTRY_DSN || '';

export const MIXPANEL_TOKEN = window._env_?.VITE_MIXPANEL_TOKEN || import.meta.env.VITE_MIXPANEL_TOKEN || '';

export const HOTJAR_ID = window._env_?.VITE_HOTJAR_ID || import.meta.env.VITE_HOTJAR_ID || '';