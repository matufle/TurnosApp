// src/theme/generateShades.ts
import type { MantineColorsTuple } from '@mantine/core';

function hexToHsl(hex: string): [number, number, number] {
  const r = parseInt(hex.slice(1, 3), 16) / 255;
  const g = parseInt(hex.slice(3, 5), 16) / 255;
  const b = parseInt(hex.slice(5, 7), 16) / 255;

  const max = Math.max(r, g, b);
  const min = Math.min(r, g, b);
  let h = 0;
  let s = 0;
  const l = (max + min) / 2;

  if (max !== min) {
    const d = max - min;
    s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
    switch (max) {
      case r: h = (g - b) / d + (g < b ? 6 : 0); break;
      case g: h = (b - r) / d + 2; break;
      case b: h = (r - g) / d + 4; break;
    }
    h /= 6;
  }

  return [h * 360, s * 100, l * 100];
}

function hslToHex(h: number, s: number, l: number): string {
  s /= 100;
  l /= 100;
  const k = (n: number) => (n + h / 30) % 12;
  const a = s * Math.min(l, 1 - l);
  const f = (n: number) => l - a * Math.max(-1, Math.min(k(n) - 3, Math.min(9 - k(n), 1)));

  const toHex = (x: number) => Math.round(x * 255).toString(16).padStart(2, '0');
  return `#${toHex(f(0))}${toHex(f(8))}${toHex(f(4))}`;
}

export function generateShades(baseHex: string): MantineColorsTuple {
  const [h, s, baseLightness] = hexToHsl(baseHex);
  const sat = Math.min(s, 85);

  const lightnessCurve = [95, 85, 75, 62, 50, baseLightness, 40, 32, 24, 16];

  // Construcción explícita del tuple de 10 posiciones — TypeScript ahora
  // puede verificar en compile-time que hay exactamente 10 elementos,
  // en vez de inferir string[] genérico como hacía el .map().
  const shades: MantineColorsTuple = [
    hslToHex(h, sat, lightnessCurve[0]),
    hslToHex(h, sat, lightnessCurve[1]),
    hslToHex(h, sat, lightnessCurve[2]),
    hslToHex(h, sat, lightnessCurve[3]),
    hslToHex(h, sat, lightnessCurve[4]),
    hslToHex(h, sat, lightnessCurve[5]),
    hslToHex(h, sat, lightnessCurve[6]),
    hslToHex(h, sat, lightnessCurve[7]),
    hslToHex(h, sat, lightnessCurve[8]),
    hslToHex(h, sat, lightnessCurve[9]),
  ];

  return shades;
}