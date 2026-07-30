// src/theme/applyTailwindBrandColor.ts
// Las páginas Tailwind-only (Servicios, Métodos de Pago, Historial de Cobros) usan los
// tokens ESTÁTICOS definidos en tailwind.css (`--color-primary`, `--color-primary-container`,
// etc.) — a diferencia de las páginas con componentes Mantine, que sí reflejan el colorHex
// dinámico del tenant vía generateShades.ts/turnifyTheme. Esto hace que el color de marca
// de un tenant "desaparezca" en esas páginas.
//
// Este módulo cierra esa brecha sobreescribiendo esos mismos custom properties en
// document.documentElement (mayor especificidad que :root en tailwind.css), reusando el
// hue/saturación del colorHex del tenant pero conservando los mismos targets de lightness
// que ya tiene el diseño estático — así el contraste texto/fondo probado en el diseño
// original (ej. on-primary-container sobre primary-container) se preserva sin reimplementar
// un generador de paleta Material Design 3 completo.
import { hexToHsl, hslToHex } from './generateShades';

const TAILWIND_PRIMARY_VARS: Record<string, number> = {
  '--color-primary': 23,
  '--color-primary-container': 42,
  '--color-on-primary-container': 16,
  '--color-on-primary-fixed-variant': 17,
};

let overridesAplicados = false;

export function applyTailwindBrandColor(baseHex: string | null) {
  const root = document.documentElement.style;

  if (!baseHex) {
    if (overridesAplicados) {
      Object.keys(TAILWIND_PRIMARY_VARS).forEach((varName) => root.removeProperty(varName));
      overridesAplicados = false;
    }
    return;
  }

  const [h, s] = hexToHsl(baseHex);
  const sat = Math.min(s, 85);

  Object.entries(TAILWIND_PRIMARY_VARS).forEach(([varName, lightness]) => {
    root.setProperty(varName, hslToHex(h, sat, lightness));
  });
  overridesAplicados = true;
}
