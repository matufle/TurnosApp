// src/components/RankingList.tsx
import { Progress } from '@mantine/core';
import type { RankingItem } from '../types/Metricas';

interface RankingListProps {
  items: RankingItem[];
  formatValor?: (valor: number) => string;
  color?: string;
}

export function RankingList({ items, formatValor, color = 'cyan' }: RankingListProps) {
  const max = Math.max(1, ...items.map((item) => item.valor));

  return (
    <div className="flex flex-col gap-4">
      {items.map((item) => (
        <div key={item.id} className="flex flex-col gap-2">
          <div className="flex justify-between items-center font-body-sm text-body-sm">
            <span className="text-on-surface">{item.nombre}</span>
            <span className="font-title-md text-on-background">
              {formatValor ? formatValor(item.valor) : item.valor}
            </span>
          </div>
          <Progress value={(item.valor / max) * 100} color={color} radius="xl" size="sm" />
        </div>
      ))}
    </div>
  );
}
