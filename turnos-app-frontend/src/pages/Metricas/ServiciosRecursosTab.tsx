// src/pages/Metricas/ServiciosRecursosTab.tsx
import { useEffect, useState } from 'react';
import { RankingList } from '../../components/RankingList';
import { PageSpinner } from '../../components/PageSpinner';
import { metricasService } from '../../api/metricasService';
import { formatMonto } from '../../utils/format';
import type { MetricasFiltro, RankingItem, ServiciosRecursosMetricas } from '../../types/Metricas';

interface ServiciosRecursosTabProps {
  filtroBase: MetricasFiltro;
}

function mergeCompletadosCancelados(completados: RankingItem[], cancelados: RankingItem[]) {
  const mapa = new Map<number, { nombre: string; completados: number; cancelados: number }>();

  for (const item of completados) {
    mapa.set(item.id, { nombre: item.nombre, completados: item.cantidad, cancelados: 0 });
  }
  for (const item of cancelados) {
    const existente = mapa.get(item.id);
    mapa.set(item.id, { nombre: item.nombre, completados: existente?.completados ?? 0, cancelados: item.cantidad });
  }

  return Array.from(mapa.entries()).map(([id, valores]) => ({ id, ...valores }));
}

export function ServiciosRecursosTab({ filtroBase }: ServiciosRecursosTabProps) {
  const [datos, setDatos] = useState<ServiciosRecursosMetricas | null>(null);
  const [loading, setLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  useEffect(() => {
    let cancelado = false;

    async function cargar() {
      setLoading(true);
      try {
        const data = await metricasService.getServiciosRecursos(filtroBase);
        if (!cancelado) {
          setDatos(data);
          setErrorMessage(null);
        }
      } catch {
        if (!cancelado) setErrorMessage('No pudimos cargar las métricas de servicios y recursos.');
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
        {errorMessage ?? 'No pudimos cargar las métricas de servicios y recursos.'}
      </div>
    );
  }

  const completadosVsCancelados = mergeCompletadosCancelados(datos.completadosPorRecurso, datos.canceladosPorRecurso);

  return (
    <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
      <div className="flex flex-col gap-6">
        <div className="flex items-center gap-2">
          <span className="material-symbols-outlined text-primary-container">spa</span>
          <h3 className="font-title-md text-title-md text-on-background">Servicios</h3>
        </div>

        <div className="bg-surface-container-lowest rounded-3xl p-6 soft-elevation border border-surface-variant">
          <h4 className="font-title-md text-title-md text-on-background mb-6">Servicios más reservados</h4>
          {datos.serviciosMasReservados.length === 0 ? (
            <p className="font-body-sm text-body-sm text-secondary">Sin datos en este período.</p>
          ) : (
            <RankingList items={datos.serviciosMasReservados} formatValor={(v) => `${v} reservas`} />
          )}
        </div>

        <div className="bg-surface-container-lowest rounded-3xl p-6 soft-elevation border border-surface-variant">
          <h4 className="font-title-md text-title-md text-on-background mb-6">Servicios más rentables</h4>
          {datos.serviciosMasRentables.length === 0 ? (
            <p className="font-body-sm text-body-sm text-secondary">Sin datos en este período.</p>
          ) : (
            <RankingList items={datos.serviciosMasRentables} formatValor={(v) => `$${formatMonto(v)}`} color="orange" />
          )}
        </div>

        <div className="bg-surface-container-lowest rounded-3xl p-6 soft-elevation border border-surface-variant">
          <h4 className="font-title-md text-title-md text-on-background mb-6">Servicios con baja demanda</h4>
          {datos.serviciosBajaDemanda.length === 0 ? (
            <p className="font-body-sm text-body-sm text-secondary">Sin datos en este período.</p>
          ) : (
            <div className="flex flex-col gap-2">
              {datos.serviciosBajaDemanda.map((servicio) => (
                <div
                  key={servicio.id}
                  className="flex justify-between items-center px-4 py-2 rounded-xl bg-surface-container-low"
                >
                  <span className="font-body-sm text-body-sm text-on-surface">{servicio.nombre}</span>
                  <span className="font-title-md text-body-sm text-secondary">{servicio.cantidad} reservas</span>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>

      <div className="flex flex-col gap-6">
        <div className="flex items-center gap-2">
          <span className="material-symbols-outlined text-primary-container">badge</span>
          <h3 className="font-title-md text-title-md text-on-background">Recursos</h3>
        </div>

        <div className="bg-surface-container-lowest rounded-3xl p-6 soft-elevation border border-surface-variant">
          <h4 className="font-title-md text-title-md text-on-background mb-6">Facturación por recurso</h4>
          {datos.facturacionPorRecurso.length === 0 ? (
            <p className="font-body-sm text-body-sm text-secondary">Sin datos en este período.</p>
          ) : (
            <RankingList items={datos.facturacionPorRecurso} formatValor={(v) => `$${formatMonto(v)}`} color="teal" />
          )}
        </div>

        <div className="bg-surface-container-lowest rounded-3xl p-6 soft-elevation border border-surface-variant">
          <h4 className="font-title-md text-title-md text-on-background mb-6">Turnos: completados vs cancelados</h4>
          {completadosVsCancelados.length === 0 ? (
            <p className="font-body-sm text-body-sm text-secondary">Sin datos en este período.</p>
          ) : (
            <div className="flex flex-col gap-4">
              {completadosVsCancelados.map((recurso) => {
                const total = recurso.completados + recurso.cancelados;
                const pctCompletados = total > 0 ? (recurso.completados / total) * 100 : 0;
                return (
                  <div key={recurso.id} className="flex flex-col gap-2">
                    <div className="flex justify-between font-body-sm text-body-sm">
                      <span className="text-on-surface">{recurso.nombre}</span>
                      <span className="text-secondary">
                        {recurso.completados} completados / {recurso.cancelados} cancelados
                      </span>
                    </div>
                    <div className="flex h-3 rounded-full overflow-hidden bg-surface-container">
                      <div style={{ width: `${pctCompletados}%` }} className="bg-primary" />
                      <div style={{ width: `${100 - pctCompletados}%` }} className="bg-error/50" />
                    </div>
                  </div>
                );
              })}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
