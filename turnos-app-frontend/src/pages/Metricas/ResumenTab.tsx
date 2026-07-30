// src/pages/Metricas/ResumenTab.tsx
import { useEffect, useState } from 'react';
import { LineChart, DonutChart } from '@mantine/charts';
import { KpiCard } from '../../components/KpiCard';
import { RankingList } from '../../components/RankingList';
import { PageSpinner } from '../../components/PageSpinner';
import { metricasService } from '../../api/metricasService';
import { formatMonto } from '../../utils/format';
import type { MetricasFiltro, ResumenMetricas } from '../../types/Metricas';

const COLOR_POR_ESTADO: Record<string, string> = {
  Completado: '#4caf50',
  Pendiente: '#00bcd4',
  Confirmado: '#3f51b5',
  EnCurso: '#2196f3',
  Ausente: '#9e9e9e',
  Cancelado: '#f44336',
};

interface ResumenTabProps {
  filtroBase: MetricasFiltro;
}

export function ResumenTab({ filtroBase }: ResumenTabProps) {
  const [datos, setDatos] = useState<ResumenMetricas | null>(null);
  const [loading, setLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  useEffect(() => {
    let cancelado = false;

    async function cargar() {
      setLoading(true);
      try {
        const data = await metricasService.getResumen(filtroBase);
        if (!cancelado) {
          setDatos(data);
          setErrorMessage(null);
        }
      } catch {
        if (!cancelado) setErrorMessage('No pudimos cargar el resumen de métricas.');
      } finally {
        if (!cancelado) setLoading(false);
      }
    }

    cargar();
    return () => {
      cancelado = true;
    };
  }, [filtroBase]);

  if (loading) {
    return (
      <PageSpinner />
    );
  }

  if (errorMessage || !datos) {
    return (
      <div
        role="alert"
        className="flex items-center gap-2 rounded-lg border border-error/30 bg-error-container px-4 py-3 font-body-sm text-body-sm text-on-error-container"
      >
        <span className="material-symbols-outlined text-[20px]">error</span>
        {errorMessage ?? 'No pudimos cargar el resumen.'}
      </div>
    );
  }

  return (
    <div className="flex flex-col gap-8">
      <section className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
        <KpiCard label="Ingresos del período" value={`$${formatMonto(datos.ingresosPeriodo)}`} icon="payments" />
        <KpiCard label="Turnos completados" value={String(datos.turnosCompletados)} icon="event_available" />
        <KpiCard label="Tasa de cancelación" value={`${datos.tasaCancelacion}%`} icon="cancel" tone="error" />
        <KpiCard
          label="Saldo pendiente"
          value={`$${formatMonto(datos.saldoPendientePeriodo)}`}
          icon="pending_actions"
          tone="tertiary"
        />
      </section>

      <section className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <div className="bg-surface-container-lowest rounded-3xl p-6 soft-elevation border border-surface-variant">
          <h4 className="font-title-md text-title-md text-on-background mb-4">Ingresos en el tiempo</h4>
          {datos.ingresosPorDia.length === 0 ? (
            <p className="font-body-sm text-body-sm text-secondary">Sin datos en este período.</p>
          ) : (
            <LineChart
              h={260}
              data={datos.ingresosPorDia}
              dataKey="etiqueta"
              series={[{ name: 'valor', color: 'cyan.6', label: 'Ingresos' }]}
              curveType="monotone"
              withDots={false}
            />
          )}
        </div>

        <div className="bg-surface-container-lowest rounded-3xl p-6 soft-elevation border border-surface-variant">
          <h4 className="font-title-md text-title-md text-on-background mb-4">Turnos por estado</h4>
          {datos.turnosPorEstado.length === 0 ? (
            <p className="font-body-sm text-body-sm text-secondary">Sin turnos en este período.</p>
          ) : (
            <DonutChart
              data={datos.turnosPorEstado.map((d) => ({
                name: d.categoria,
                value: d.cantidad,
                color: COLOR_POR_ESTADO[d.categoria] ?? '#9e9e9e',
              }))}
              withLabelsLine
              withLabels
            />
          )}
        </div>
      </section>

      <section className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <div className="bg-surface-container-lowest rounded-3xl p-6 soft-elevation border border-surface-variant">
          <h4 className="font-title-md text-title-md text-on-background mb-6">Top servicios por ingresos</h4>
          {datos.topServiciosPorIngresos.length === 0 ? (
            <p className="font-body-sm text-body-sm text-secondary">Sin datos en este período.</p>
          ) : (
            <RankingList items={datos.topServiciosPorIngresos} formatValor={(v) => `$${formatMonto(v)}`} />
          )}
        </div>

        <div className="bg-surface-container-lowest rounded-3xl p-6 soft-elevation border border-surface-variant">
          <h4 className="font-title-md text-title-md text-on-background mb-6">Top recursos por turnos</h4>
          {datos.topRecursosPorTurnos.length === 0 ? (
            <p className="font-body-sm text-body-sm text-secondary">Sin datos en este período.</p>
          ) : (
            <RankingList items={datos.topRecursosPorTurnos} formatValor={(v) => `${v} turnos`} color="teal" />
          )}
        </div>
      </section>
    </div>
  );
}
