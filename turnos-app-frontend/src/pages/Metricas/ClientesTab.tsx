// src/pages/Metricas/ClientesTab.tsx
import { useEffect, useState } from 'react';
import { BarChart, DonutChart } from '@mantine/charts';
import { KpiCard } from '../../components/KpiCard';
import { metricasService } from '../../api/metricasService';
import { formatMonto } from '../../utils/format';
import type { ClientesMetricas, MetricasFiltro } from '../../types/Metricas';

const COLOR_DISTRIBUCION: Record<string, string> = {
  Nuevos: '#006876',
  Recurrentes: '#00bcd4',
  Inactivos: '#ffb77b',
};

interface ClientesTabProps {
  filtroBase: MetricasFiltro;
}

export function ClientesTab({ filtroBase }: ClientesTabProps) {
  const [datos, setDatos] = useState<ClientesMetricas | null>(null);
  const [loading, setLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  useEffect(() => {
    let cancelado = false;

    async function cargar() {
      setLoading(true);
      try {
        const data = await metricasService.getClientes(filtroBase);
        if (!cancelado) {
          setDatos(data);
          setErrorMessage(null);
        }
      } catch {
        if (!cancelado) setErrorMessage('No pudimos cargar las métricas de clientes.');
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
        {errorMessage ?? 'No pudimos cargar las métricas de clientes.'}
      </div>
    );
  }

  return (
    <div className="flex flex-col gap-8">
      <section className="grid grid-cols-1 sm:grid-cols-3 gap-6">
        <KpiCard label="Clientes nuevos del período" value={String(datos.clientesNuevos)} icon="person_add" />
        <KpiCard label="% Clientes recurrentes" value={`${datos.porcentajeRecurrentes}%`} icon="sync" />
        <KpiCard
          label="Clientes inactivos (+60 días)"
          value={String(datos.clientesInactivos)}
          icon="history"
          tone="error"
        />
      </section>

      <section className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div className="lg:col-span-2 bg-surface-container-lowest rounded-3xl p-6 soft-elevation border border-surface-variant">
          <h4 className="font-title-md text-title-md text-on-background mb-4">Clientes nuevos por mes</h4>
          {datos.nuevosPorMes.length === 0 ? (
            <p className="font-body-sm text-body-sm text-secondary">Sin datos en este período.</p>
          ) : (
            <BarChart
              h={260}
              data={datos.nuevosPorMes}
              dataKey="etiqueta"
              series={[{ name: 'valor', color: 'cyan.6', label: 'Clientes nuevos' }]}
            />
          )}
        </div>

        <div className="bg-surface-container-lowest rounded-3xl p-6 soft-elevation border border-surface-variant">
          <h4 className="font-title-md text-title-md text-on-background mb-4">Distribución de clientes</h4>
          {datos.nuevosRecurrentesInactivos.every((d) => d.cantidad === 0) ? (
            <p className="font-body-sm text-body-sm text-secondary">Sin datos en este período.</p>
          ) : (
            <DonutChart
              data={datos.nuevosRecurrentesInactivos.map((d) => ({
                name: d.categoria,
                value: d.cantidad,
                color: COLOR_DISTRIBUCION[d.categoria] ?? '#9e9e9e',
              }))}
              withLabelsLine
              withLabels
            />
          )}
        </div>
      </section>

      <section className="bg-surface-container-lowest rounded-3xl soft-elevation overflow-hidden border border-surface-variant">
        <div className="p-6 border-b border-surface-variant">
          <h4 className="font-title-md text-title-md text-on-background">Top clientes por facturación</h4>
        </div>
        {datos.topClientesPorFacturacion.length === 0 ? (
          <p className="p-6 font-body-sm text-body-sm text-secondary">Sin datos en este período.</p>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left border-collapse">
              <thead>
                <tr className="bg-surface-container-low border-b border-outline-variant">
                  <th className="py-3 px-6 font-label-md text-label-md text-secondary">Cliente</th>
                  <th className="py-3 px-6 font-label-md text-label-md text-secondary">Cobros</th>
                  <th className="py-3 px-6 font-label-md text-label-md text-secondary text-right">Total facturado</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-surface-variant">
                {datos.topClientesPorFacturacion.map((cliente) => (
                  <tr key={cliente.id} className="hover:bg-surface-container transition-colors">
                    <td className="py-3 px-6">
                      <div className="flex items-center gap-3">
                        <div className="w-8 h-8 rounded-full bg-primary/10 flex items-center justify-center font-title-md text-body-sm text-primary">
                          {cliente.nombre.charAt(0).toUpperCase()}
                        </div>
                        <span className="font-title-md text-body-sm text-on-surface">{cliente.nombre}</span>
                      </div>
                    </td>
                    <td className="py-3 px-6 font-body-sm text-body-sm text-secondary">{cliente.cantidad}</td>
                    <td className="py-3 px-6 font-title-md text-body-sm text-primary text-right">
                      ${formatMonto(cliente.valor)}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>
    </div>
  );
}
