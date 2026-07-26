// src/pages/Metricas/TurnosTab.tsx
import { Fragment, useEffect, useState } from 'react';
import { LineChart } from '@mantine/charts';
import { KpiCard } from '../../components/KpiCard';
import { RankingList } from '../../components/RankingList';
import { metricasService } from '../../api/metricasService';
import type { MetricasFiltro, PuntoSerie, TurnosMetricas } from '../../types/Metricas';

const DIAS_ORDEN = [1, 2, 3, 4, 5, 6, 0]; // Lun..Dom (System.DayOfWeek: 0=Domingo)
const DIAS_LABEL = ['Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb', 'Dom'];
const HORAS = Array.from({ length: 24 }, (_, h) => h);

function mergeSeries(creados: PuntoSerie[], completados: PuntoSerie[]) {
  const mapa = new Map<string, { creados: number; completados: number }>();

  for (const p of creados) {
    mapa.set(p.etiqueta, { creados: p.valor, completados: 0 });
  }
  for (const p of completados) {
    const existente = mapa.get(p.etiqueta);
    mapa.set(p.etiqueta, { creados: existente?.creados ?? 0, completados: p.valor });
  }

  return Array.from(mapa.entries())
    .map(([etiqueta, valores]) => ({ etiqueta, ...valores }))
    .sort((a, b) => a.etiqueta.localeCompare(b.etiqueta));
}

interface TurnosTabProps {
  filtroBase: MetricasFiltro;
}

export function TurnosTab({ filtroBase }: TurnosTabProps) {
  const [datos, setDatos] = useState<TurnosMetricas | null>(null);
  const [loading, setLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  useEffect(() => {
    let cancelado = false;

    async function cargar() {
      setLoading(true);
      try {
        const data = await metricasService.getTurnos(filtroBase);
        if (!cancelado) {
          setDatos(data);
          setErrorMessage(null);
        }
      } catch {
        if (!cancelado) setErrorMessage('No pudimos cargar las métricas de turnos.');
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
      <div className="flex items-center justify-center py-24">
        <span className="material-symbols-outlined animate-spin text-primary text-4xl">progress_activity</span>
      </div>
    );
  }

  if (errorMessage || !datos) {
    return (
      <div
        role="alert"
        className="flex items-center gap-2 rounded-lg border border-error/30 bg-error-container px-4 py-3 font-body-sm text-body-sm text-on-error-container"
      >
        <span className="material-symbols-outlined text-[20px]">error</span>
        {errorMessage ?? 'No pudimos cargar las métricas de turnos.'}
      </div>
    );
  }

  const maxCantidad = Math.max(1, ...datos.heatmap.map((c) => c.cantidad));
  const mapaCeldas = new Map(datos.heatmap.map((c) => [`${c.diaSemana}-${c.hora}`, c.cantidad]));
  const serieEvolucion = mergeSeries(datos.creados, datos.completados);

  return (
    <div className="flex flex-col gap-8">
      <section className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
        <KpiCard label="Turnos totales" value={String(datos.turnosTotales)} icon="event" />
        <KpiCard label="Tasa de cancelación" value={`${datos.tasaCancelacion}%`} icon="cancel" tone="error" />
        <KpiCard label="Tasa de ausentismo" value={`${datos.tasaAusentismo}%`} icon="event_busy" tone="tertiary" />
        <KpiCard
          label="Anticipación promedio de reserva"
          value={`${datos.anticipacionPromedioHoras}h`}
          icon="schedule"
        />
      </section>

      <section className="bg-surface-container-lowest rounded-3xl p-6 soft-elevation border border-surface-variant">
        <h4 className="font-title-md text-title-md text-on-background mb-4">Turnos por día de semana y franja horaria</h4>
        {datos.heatmap.length === 0 ? (
          <p className="font-body-sm text-body-sm text-secondary">Sin turnos en este período.</p>
        ) : (
          <div className="overflow-x-auto">
            <div className="min-w-[880px]">
              <div className="grid gap-1" style={{ gridTemplateColumns: '48px repeat(24, minmax(0, 1fr))' }}>
                <div />
                {HORAS.map((h) => (
                  <div key={h} className="text-center font-label-md text-label-md text-secondary">
                    {h}h
                  </div>
                ))}
                {DIAS_ORDEN.map((dia, idx) => (
                  <Fragment key={dia}>
                    <div className="flex items-center font-body-sm text-body-sm text-on-surface-variant h-8">
                      {DIAS_LABEL[idx]}
                    </div>
                    {HORAS.map((hora) => {
                      const cantidad = mapaCeldas.get(`${dia}-${hora}`) ?? 0;
                      const intensidad = cantidad / maxCantidad;
                      return (
                        <div
                          key={hora}
                          className="h-8 rounded"
                          style={{
                            backgroundColor:
                              cantidad === 0 ? 'var(--color-surface-container)' : `rgba(0, 104, 118, ${0.15 + intensidad * 0.85})`,
                          }}
                          title={`${DIAS_LABEL[idx]} ${hora}h — ${cantidad} turnos`}
                        />
                      );
                    })}
                  </Fragment>
                ))}
              </div>
            </div>
          </div>
        )}
      </section>

      <section className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <div className="bg-surface-container-lowest rounded-3xl p-6 soft-elevation border border-surface-variant">
          <h4 className="font-title-md text-title-md text-on-background mb-4">Evolución de turnos</h4>
          {serieEvolucion.length === 0 ? (
            <p className="font-body-sm text-body-sm text-secondary">Sin datos en este período.</p>
          ) : (
            <LineChart
              h={260}
              data={serieEvolucion}
              dataKey="etiqueta"
              series={[
                { name: 'creados', color: 'cyan.4', label: 'Creados' },
                { name: 'completados', color: 'cyan.8', label: 'Completados' },
              ]}
              curveType="monotone"
              withDots={false}
            />
          )}
        </div>

        <div className="bg-surface-container-lowest rounded-3xl p-6 soft-elevation border border-surface-variant">
          <h4 className="font-title-md text-title-md text-on-background mb-6">Ocupación por recurso (horas)</h4>
          {datos.ocupacionPorRecurso.length === 0 ? (
            <p className="font-body-sm text-body-sm text-secondary">Sin datos en este período.</p>
          ) : (
            <RankingList items={datos.ocupacionPorRecurso} formatValor={(v) => `${v}h`} color="teal" />
          )}
        </div>
      </section>
    </div>
  );
}
