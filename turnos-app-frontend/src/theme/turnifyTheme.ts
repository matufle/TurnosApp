import type { MantineColorsTuple } from '@mantine/core';
import { createTheme } from '@mantine/core';

const cyanTurnify: MantineColorsTuple = [
  '#e6f7ff', '#b3e7ff', '#80d6ff', '#4dc6ff', '#26b8ff',
  '#0EA5E9', // shade 5 — tu color exacto
  '#0b8fca', '#0879ab', '#05638c', '#034d6d'
];

export const theme = createTheme({
  colors: { cyan: cyanTurnify },
  primaryColor: 'cyan',
  primaryShade: 5,
  fontFamily: 'Inter, sans-serif',
  headings: { fontFamily: 'Inter, sans-serif', fontWeight: '700' },
  defaultRadius: 'md',
});