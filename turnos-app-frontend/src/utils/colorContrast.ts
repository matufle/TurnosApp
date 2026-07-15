// src/utils/colorContrast.ts

/**
 * Calcula si el texto debe ser blanco o negro según la luminosidad
 * relativa del color de fondo (fórmula de contraste W3C/WCAG).
 */
export function getContrastTextColor(hex: string): string {
  const cleanHex = hex.replace('#', '');
  const r = parseInt(cleanHex.substring(0, 2), 16) / 255;
  const g = parseInt(cleanHex.substring(2, 4), 16) / 255;
  const b = parseInt(cleanHex.substring(4, 6), 16) / 255;

  const linear = (c: number) => (c <= 0.03928 ? c / 12.92 : ((c + 0.055) / 1.055) ** 2.4);

  const luminance = 0.2126 * linear(r) + 0.7152 * linear(g) + 0.0722 * linear(b);

  // Umbral estándar: por encima de 0.5 el fondo es "claro" → texto negro
  return luminance > 0.5 ? '#1a1a1a' : '#ffffff';
}