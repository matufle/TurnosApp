// src/components/KpiCard.tsx
// Extraído del markup repetido en HistorialCobrosPage.tsx (tarjetas de KPI).
interface KpiCardProps {
  label: string;
  value: string;
  icon: string;
  tone?: 'primary' | 'error' | 'tertiary';
  // No renderiza nada — usar para KPIs de ganancia neta/comisión sin permiso VerGananciaNeta.
  hidden?: boolean;
}

const toneClasses: Record<NonNullable<KpiCardProps['tone']>, string> = {
  primary: 'text-primary bg-primary/10',
  error: 'text-error bg-error/10',
  tertiary: 'text-tertiary bg-tertiary/10',
};

export function KpiCard({ label, value, icon, tone = 'primary', hidden = false }: KpiCardProps) {
  if (hidden) return null;

  return (
    <div className="bg-surface-container-lowest rounded-3xl p-6 soft-elevation border border-surface-variant flex flex-col gap-2 relative overflow-hidden">
      <div className="flex justify-between items-center">
        <span className="font-body-sm text-body-sm text-secondary">{label}</span>
        <span className={`material-symbols-outlined p-2 rounded-full ${toneClasses[tone]}`}>{icon}</span>
      </div>
      <div className="font-display-lg text-display-lg text-on-background">{value}</div>
    </div>
  );
}
