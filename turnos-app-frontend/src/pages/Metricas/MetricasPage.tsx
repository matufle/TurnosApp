// src/pages/Metricas/MetricasPage.tsx
import { Tabs } from '@mantine/core';
import { DatePickerInput } from '@mantine/dates';
import { useSearchParams } from 'react-router-dom';
import { useMetricasFiltro, type PresetRango } from './useMetricasFiltro';
import { ResumenTab } from './ResumenTab';
import { IngresosTab } from './IngresosTab';
import { TurnosTab } from './TurnosTab';
import { ClientesTab } from './ClientesTab';
import { ServiciosRecursosTab } from './ServiciosRecursosTab';

const TABS = ['resumen', 'ingresos', 'turnos', 'clientes', 'servicios-recursos'] as const;
type TabValue = (typeof TABS)[number];

const PRESETS: { value: PresetRango; label: string }[] = [
  { value: 'hoy', label: 'Hoy' },
  { value: 'semana', label: 'Semana' },
  { value: 'mes', label: 'Mes' },
  { value: 'trimestre', label: 'Trimestre' },
  { value: 'anio', label: 'Año' },
];

export function MetricasPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const { preset, fechaDesde, fechaHasta, setPreset, setRangoPersonalizado, filtroBase } = useMetricasFiltro();

  const tabPedida = searchParams.get('tab');
  const tabActual: TabValue = TABS.includes(tabPedida as TabValue) ? (tabPedida as TabValue) : 'resumen';

  function cambiarTab(value: string | null) {
    if (!value) return;
    setSearchParams({ tab: value }, { replace: true });
  }

  return (
    <div className="flex flex-col gap-8 pb-12">
      <div className="flex flex-col md:flex-row justify-between items-start md:items-end gap-6">
        <div>
          <h1 className="font-headline-lg text-headline-lg text-on-background">Métricas</h1>
          <p className="font-body-lg text-body-lg text-secondary mt-2">Panorama general del negocio.</p>
        </div>
        <div className="flex flex-wrap gap-2 items-center w-full md:w-auto">
          {PRESETS.map((p) => (
            <button
              key={p.value}
              onClick={() => setPreset(p.value)}
              className={`px-4 py-2 rounded-full font-label-md text-label-md transition-colors ${
                preset === p.value
                  ? 'bg-primary-container text-on-primary-container'
                  : 'bg-surface-container text-on-surface hover:bg-secondary-container'
              }`}
            >
              {p.label}
            </button>
          ))}
          <DatePickerInput
            placeholder="Desde"
            value={fechaDesde}
            onChange={(value) => setRangoPersonalizado(value, fechaHasta)}
            clearable
            className="w-36"
          />
          <DatePickerInput
            placeholder="Hasta"
            value={fechaHasta}
            onChange={(value) => setRangoPersonalizado(fechaDesde, value)}
            clearable
            className="w-36"
          />
        </div>
      </div>

      <Tabs value={tabActual} onChange={cambiarTab} keepMounted={false}>
        <Tabs.List data-tour="metricas-tabs">
          <Tabs.Tab value="resumen">Resumen</Tabs.Tab>
          <Tabs.Tab value="ingresos">Ingresos</Tabs.Tab>
          <Tabs.Tab value="turnos">Turnos</Tabs.Tab>
          <Tabs.Tab value="clientes">Clientes</Tabs.Tab>
          <Tabs.Tab value="servicios-recursos">Servicios &amp; Recursos</Tabs.Tab>
        </Tabs.List>

        <Tabs.Panel value="resumen" pt="lg">
          <ResumenTab filtroBase={filtroBase} />
        </Tabs.Panel>
        <Tabs.Panel value="ingresos" pt="lg">
          <IngresosTab filtroBase={filtroBase} />
        </Tabs.Panel>
        <Tabs.Panel value="turnos" pt="lg">
          <TurnosTab filtroBase={filtroBase} />
        </Tabs.Panel>
        <Tabs.Panel value="clientes" pt="lg">
          <ClientesTab filtroBase={filtroBase} />
        </Tabs.Panel>
        <Tabs.Panel value="servicios-recursos" pt="lg">
          <ServiciosRecursosTab filtroBase={filtroBase} />
        </Tabs.Panel>
      </Tabs>
    </div>
  );
}
