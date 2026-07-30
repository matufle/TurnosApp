// src/components/PageSpinner.tsx
// Spinner de carga a página completa, extraído del markup idéntico repetido
// en cada página (Servicios, Recursos, Clientes, Turnos, Métricas, etc.).
interface PageSpinnerProps {
  size?: 'sm' | 'lg';
}

export function PageSpinner({ size = 'lg' }: PageSpinnerProps) {
  return (
    <div className="flex items-center justify-center py-24">
      <span
        className={`material-symbols-outlined animate-spin text-primary ${
          size === 'lg' ? 'text-4xl' : 'text-2xl'
        }`}
      >
        progress_activity
      </span>
    </div>
  );
}
