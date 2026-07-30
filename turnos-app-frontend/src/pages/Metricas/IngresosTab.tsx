// src/pages/Metricas/IngresosTab.tsx
import { useEffect, useState } from 'react';
import { AreaChart, BarChart, DonutChart } from '@mantine/charts';
import { KpiCard } from '../../components/KpiCard';
import { PageSpinner } from '../../components/PageSpinner';
import { usePermission } from '../../hooks/usePermission';
import { metricasService } from '../../api/metricasService';
import { cobrosService } from '../../api/cobrosService';
import { formatMonto } from '../../utils/format';
import type { MetricasFiltro, IngresosMetricas } from '../../types/Metricas';
import type { HistorialCobros } from '../../types/Cobro';

const COLOR_ESTADO_PAGO: Record<string, string> = {
  Pagado: '#4caf50',
  Parcial: '#f19640',
  SinCobrar: '#ba1a1a',
};

const TAMANO_PAGINA = 10;

interface IngresosTabProps {
  filtroBase: MetricasFiltro;
}

export function IngresosTab({ filtroBase }: IngresosTabProps) {
  const puedeVerGananciaNeta = usePermission('VerGananciaNeta');
  const [datos, setDatos] = useState<IngresosMetricas | null>(null);
  const [historial, setHistorial] = useState<HistorialCobros | null>(null);
  const [loading, setLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [pagina, setPagina] = useState(1);

  useEffect(() => {
    setPagina(1);
  }, [filtroBase]);

  useEffect(() => {
    let cancelado = false;

    async function cargar() {
      setLoading(true);
      try {
        const [metricas, historialCobros] = await Promise.all([
          metricasService.getIngresos(filtroBase),
          cobrosService.getHistorial({ ...filtroBase, pagina, tamanoPagina: TAMANO_PAGINA }),
        ]);
        if (!cancelado) {
          setDatos(metricas);
          setHistorial(historialCobros);
          setErrorMessage(null);
        }
      } catch {
        if (!cancelado) setErrorMessage('No pudimos cargar las métricas de ingresos.');
      } finally {
        if (!cancelado) setLoading(false);
      }
    }

    cargar();
    return () => {
      cancelado = true;
    };
  }, [filtroBase, pagina]);

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
        {errorMessage ?? 'No pudimos cargar las métricas de ingresos.'}
      </div>
    );
  }

  const totalPages = historial ? Math.max(1, Math.ceil(historial.totalCount / historial.tamanoPagina)) : 1;

  return (
    <div className="flex flex-col gap-8">
      <section className="grid grid-cols-1 sm:grid-cols-3 gap-6">
        <KpiCard label="Ingresos totales" value={`$${formatMonto(datos.ingresosTotales)}`} icon="payments" />
        <KpiCard
          label="Ganancia neta"
          value={datos.gananciaNeta !== null ? `$${formatMonto(datos.gananciaNeta)}` : '—'}
          icon="account_balance_wallet"
          tone="tertiary"
          hidden={!puedeVerGananciaNeta}
        />
        <KpiCard label="Ticket promedio" value={`$${formatMonto(datos.ticketPromedio)}`} icon="confirmation_number" />
      </section>

      <section className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <div className="bg-surface-container-lowest rounded-3xl p-6 soft-elevation border border-surface-variant lg:col-span-2">
          <h4 className="font-title-md text-title-md text-on-background mb-4">
            {puedeVerGananciaNeta ? 'Ingresos vs comisión vs ganancia neta' : 'Ingresos en el tiempo'}
          </h4>
          {datos.ingresosComisionGanancia.length === 0 ? (
            <p className="font-body-sm text-body-sm text-secondary">Sin datos en este período.</p>
          ) : (
            <AreaChart
              h={280}
              data={datos.ingresosComisionGanancia.map((s) => ({
                etiqueta: s.etiqueta,
                ingresos: s.ingresos,
                comision: s.comision ?? 0,
                gananciaNeta: s.gananciaNeta ?? 0,
              }))}
              dataKey="etiqueta"
              type="stacked"
              series={
                puedeVerGananciaNeta
                  ? [
                      { name: 'gananciaNeta', color: 'cyan.6', label: 'Ganancia neta' },
                      { name: 'comision', color: 'orange.5', label: 'Comisión' },
                    ]
                  : [{ name: 'ingresos', color: 'cyan.6', label: 'Ingresos' }]
              }
            />
          )}
        </div>

        <div className="bg-surface-container-lowest rounded-3xl p-6 soft-elevation border border-surface-variant">
          <h4 className="font-title-md text-title-md text-on-background mb-4">Ingresos por método de pago</h4>
          {datos.ingresosPorMetodoPago.length === 0 ? (
            <p className="font-body-sm text-body-sm text-secondary">Sin datos en este período.</p>
          ) : (
            <BarChart
              h={240}
              data={datos.ingresosPorMetodoPago}
              dataKey="nombre"
              series={[{ name: 'valor', color: 'cyan.6', label: 'Ingresos' }]}
            />
          )}
        </div>

        <div className="bg-surface-container-lowest rounded-3xl p-6 soft-elevation border border-surface-variant">
          <h4 className="font-title-md text-title-md text-on-background mb-4">Estado de pago de los turnos</h4>
          {datos.estadoPagoTurnos.every((d) => d.cantidad === 0) ? (
            <p className="font-body-sm text-body-sm text-secondary">Sin turnos en este período.</p>
          ) : (
            <DonutChart
              data={datos.estadoPagoTurnos.map((d) => ({
                name: d.categoria,
                value: d.cantidad,
                color: COLOR_ESTADO_PAGO[d.categoria] ?? '#9e9e9e',
              }))}
              withLabelsLine
              withLabels
            />
          )}
        </div>
      </section>

      <section className="bg-surface-container-lowest rounded-3xl soft-elevation overflow-hidden border border-surface-variant">
        <div className="p-6 border-b border-surface-variant">
          <h4 className="font-title-md text-title-md text-on-background">Detalle de cobros</h4>
        </div>
        {!historial || historial.items.length === 0 ? (
          <p className="p-6 font-body-sm text-body-sm text-secondary">Sin cobros en este período.</p>
        ) : (
          <>
            <div className="overflow-x-auto">
              <table className="w-full text-left border-collapse">
                <thead>
                  <tr className="bg-surface-container-low border-b border-outline-variant">
                    <th className="py-3 px-6 font-label-md text-label-md text-secondary">Cliente / Turno</th>
                    <th className="py-3 px-6 font-label-md text-label-md text-secondary">Fecha</th>
                    <th className="py-3 px-6 font-label-md text-label-md text-secondary">Método</th>
                    <th className="py-3 px-6 font-label-md text-label-md text-secondary text-right">Precio Final</th>
                    {puedeVerGananciaNeta && (
                      <th className="py-3 px-6 font-label-md text-label-md text-secondary text-right">Ganancia</th>
                    )}
                  </tr>
                </thead>
                <tbody className="divide-y divide-surface-variant">
                  {historial.items.map((item) => (
                    <tr key={item.id} className="hover:bg-surface-container transition-colors">
                      <td className="py-3 px-6">
                        <div className="font-title-md text-body-sm text-on-surface">{item.clienteNombreCompleto}</div>
                        <div className="font-body-sm text-label-md text-secondary">
                          T-{item.turnoId} • {item.serviciosResumen}
                        </div>
                      </td>
                      <td className="py-3 px-6 font-body-sm text-body-sm text-on-surface-variant">
                        {new Date(item.fechaHoraTurno).toLocaleDateString('es-AR', { day: '2-digit', month: 'short' })}
                      </td>
                      <td className="py-3 px-6 font-body-sm text-body-sm text-on-surface-variant">
                        {item.nombreMetodoPagoSnapshot}
                      </td>
                      <td className="py-3 px-6 font-title-md text-body-sm text-on-background text-right">
                        ${formatMonto(item.precioFinal)}
                      </td>
                      {puedeVerGananciaNeta && (
                        <td className="py-3 px-6 font-title-md text-body-sm text-primary text-right">
                          ${formatMonto(item.gananciaNeta ?? 0)}
                        </td>
                      )}
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
            <div className="px-6 py-4 border-t border-surface-variant flex justify-between items-center">
              <span className="font-body-sm text-body-sm text-secondary">
                Página {historial.pagina} de {totalPages}
              </span>
              <div className="flex gap-2">
                <button
                  disabled={historial.pagina <= 1}
                  onClick={() => setPagina((p) => Math.max(1, p - 1))}
                  className="w-8 h-8 rounded flex items-center justify-center border border-outline-variant text-secondary hover:bg-surface-container-low transition-colors disabled:opacity-40 disabled:cursor-not-allowed"
                >
                  <span className="material-symbols-outlined text-[18px]">chevron_left</span>
                </button>
                <button
                  disabled={historial.pagina >= totalPages}
                  onClick={() => setPagina((p) => Math.min(totalPages, p + 1))}
                  className="w-8 h-8 rounded flex items-center justify-center border border-outline-variant text-secondary hover:bg-surface-container-low transition-colors disabled:opacity-40 disabled:cursor-not-allowed"
                >
                  <span className="material-symbols-outlined text-[18px]">chevron_right</span>
                </button>
              </div>
            </div>
          </>
        )}
      </section>
    </div>
  );
}
