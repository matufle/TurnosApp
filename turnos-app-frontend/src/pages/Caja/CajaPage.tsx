// src/pages/Caja/CajaPage.tsx
import { useCallback, useEffect, useState } from 'react';
import axios from 'axios';
import { NumberInput, Textarea, Button, Tabs } from '@mantine/core';
import { useDisclosure } from '@mantine/hooks';
import { useForm } from '@mantine/form';
import { cajaService } from '../../api/cajaService';
import { PageSpinner } from '../../components/PageSpinner';
import { usePermission } from '../../hooks/usePermission';
import { RegistrarMovimientoModal } from './RegistrarMovimientoModal';
import { CerrarCajaModal } from './CerrarCajaModal';
import { HistorialCajaTab } from './HistorialCajaTab';
import type { SesionCaja, TipoMovimientoCaja } from '../../types/Caja';

function formatMonto(valor: number) {
  return valor.toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}

function formatHora(iso: string) {
  return new Date(iso).toLocaleTimeString('es-AR', { hour: '2-digit', minute: '2-digit' });
}

function formatFechaHora(iso: string) {
  return new Date(iso).toLocaleString('es-AR', { day: '2-digit', month: 'short', hour: '2-digit', minute: '2-digit' });
}

function iconoPorNombre(nombre: string) {
  const n = nombre.toLowerCase();
  if (n.includes('efectivo')) return 'payments';
  if (n.includes('tarjeta') || n.includes('credito') || n.includes('crédito')) return 'credit_card';
  if (n.includes('transferencia') || n.includes('banco')) return 'account_balance';
  return 'payments';
}

interface AbrirFormValues {
  montoInicial: number;
  observaciones: string;
}

export function CajaPage() {
  const puedeGestionar = usePermission('GestionarCaja');
  const [sesion, setSesion] = useState<SesionCaja | null>(null);
  const [loading, setLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [abriendo, setAbriendo] = useState(false);
  const [tabActivo, setTabActivo] = useState<string | null>('actual');
  const [tipoMovimientoInicial, setTipoMovimientoInicial] = useState<TipoMovimientoCaja>('Ingreso');

  const [movimientoModalOpened, { open: abrirMovimientoModal, close: cerrarMovimientoModal }] = useDisclosure(false);
  const [cerrarModalOpened, { open: abrirCerrarModal, close: cerrarCerrarModal }] = useDisclosure(false);

  const form = useForm<AbrirFormValues>({
    initialValues: { montoInicial: 0, observaciones: '' },
    validate: {
      montoInicial: (value) => (value >= 0 ? null : 'No puede ser negativo'),
    },
  });

  const cargarSesion = useCallback(async () => {
    try {
      const data = await cajaService.getSesionAbierta();
      setSesion(data);
      setErrorMessage(null);
    } catch {
      setErrorMessage('No pudimos cargar el estado de la caja.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    cargarSesion();
  }, [cargarSesion]);

  const handleAbrir = async (values: AbrirFormValues) => {
    setAbriendo(true);
    setErrorMessage(null);
    try {
      const nueva = await cajaService.abrir({
        montoInicial: values.montoInicial,
        observaciones: values.observaciones || undefined,
      });
      setSesion(nueva);
      form.reset();
    } catch (error) {
      if (axios.isAxiosError(error) && error.response?.data?.detail) {
        setErrorMessage(error.response.data.detail);
      } else {
        setErrorMessage('No pudimos abrir la caja.');
      }
    } finally {
      setAbriendo(false);
    }
  };

  const handleAbrirMovimiento = (tipo: TipoMovimientoCaja) => {
    setTipoMovimientoInicial(tipo);
    abrirMovimientoModal();
  };

  if (loading) {
    return <PageSpinner />;
  }

  const totalIngresos = sesion?.movimientos.filter((m) => m.tipo === 'Ingreso').reduce((acc, m) => acc + m.monto, 0) ?? 0;
  const totalEfectivo = sesion?.desglosePorMedioPago.find((d) => d.esEfectivo)?.total ?? 0;
  const totalOtrosMedios = sesion?.desglosePorMedioPago.filter((d) => !d.esEfectivo).reduce((acc, d) => acc + d.total, 0) ?? 0;
  const totalFondos = totalEfectivo + totalOtrosMedios;
  const pctEfectivo = totalFondos > 0 ? Math.max(0, Math.min(100, (totalEfectivo / totalFondos) * 100)) : 0;
  const pctOtros = totalFondos > 0 ? Math.max(0, Math.min(100, (totalOtrosMedios / totalFondos) * 100)) : 0;

  return (
    <div className="flex flex-col gap-10 pb-12">
      <header>
        <h1 className="font-headline-lg text-headline-lg text-on-surface">Caja</h1>
        <p className="font-body-lg text-body-lg text-secondary mt-2">Apertura, movimientos y cierre de caja.</p>
      </header>

      {errorMessage && (
        <div
          role="alert"
          className="flex items-center gap-2 rounded-lg border border-error/30 bg-error-container px-4 py-3 font-body-sm text-body-sm text-on-error-container"
        >
          <span className="material-symbols-outlined text-[20px]">error</span>
          {errorMessage}
        </div>
      )}

      <Tabs value={tabActivo} onChange={setTabActivo} color="cyan">
        <Tabs.List>
          <Tabs.Tab value="actual">Caja actual</Tabs.Tab>
          <Tabs.Tab value="historial">Historial</Tabs.Tab>
        </Tabs.List>

        <Tabs.Panel value="actual" pt="lg">
          {!sesion ? (
            <div className="w-full max-w-[600px] mx-auto bg-surface-container-lowest rounded-3xl soft-elevation p-8 md:p-12 relative overflow-hidden">
              <div className="flex flex-col items-center text-center mb-10 relative z-10">
                <div className="w-24 h-24 bg-secondary-container text-primary rounded-3xl flex items-center justify-center mb-6 shadow-sm border border-outline-variant/20">
                  <span className="material-symbols-outlined text-[48px]" style={{ fontVariationSettings: "'FILL' 1" }}>
                    point_of_sale
                  </span>
                </div>
                <h2 className="font-headline-lg text-headline-lg text-on-surface mb-2">Apertura de Caja</h2>
                <p className="font-body-lg text-body-lg text-secondary">
                  {puedeGestionar
                    ? 'Iniciá la jornada configurando el saldo inicial en efectivo.'
                    : 'Todavía no se abrió la caja hoy.'}
                </p>
              </div>

              {puedeGestionar && (
                <form onSubmit={form.onSubmit(handleAbrir)} className="space-y-6 relative z-10">
                  <div className="space-y-2">
                    <label className="font-label-md text-label-md text-outline uppercase tracking-wider block ml-1">
                      Fecha
                    </label>
                    <div className="relative">
                      <span className="material-symbols-outlined absolute left-4 top-1/2 -translate-y-1/2 text-outline text-[20px]">
                        calendar_today
                      </span>
                      <input
                        readOnly
                        value={new Date().toLocaleDateString('es-AR', { day: '2-digit', month: 'short', year: 'numeric' })}
                        className="w-full pl-12 pr-4 py-3 bg-surface-container-low border border-outline-variant rounded-xl font-body-lg text-on-surface focus:outline-none cursor-default"
                      />
                    </div>
                  </div>

                  <NumberInput
                    label="Saldo inicial de efectivo"
                    description="Fondo fijo / vuelto con el que arranca la caja"
                    min={0}
                    decimalScale={2}
                    prefix="$"
                    size="md"
                    {...form.getInputProps('montoInicial')}
                  />

                  <Textarea
                    label="Notas opcionales"
                    placeholder="Observaciones sobre el estado inicial de la caja..."
                    autosize
                    minRows={3}
                    {...form.getInputProps('observaciones')}
                  />

                  <Button
                    type="submit"
                    loading={abriendo}
                    fullWidth
                    size="lg"
                    color="cyan"
                    leftSection={<span className="material-symbols-outlined">lock_open</span>}
                  >
                    Abrir caja
                  </Button>
                </form>
              )}
            </div>
          ) : (
            <div className="flex flex-col gap-8">
              <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
                <div className="flex items-center gap-4">
                  <span className="inline-flex items-center gap-1.5 px-3 py-1 bg-green-100 text-green-700 rounded-full font-label-md text-label-md">
                    <span className="w-2 h-2 rounded-full bg-green-600" />
                    Abierta desde {formatFechaHora(sesion.fechaApertura)} por {sesion.usuarioAperturaNombre}
                  </span>
                </div>
                {puedeGestionar && (
                  <div className="flex gap-4">
                    <button
                      onClick={() => handleAbrirMovimiento('Ingreso')}
                      className="flex-1 md:flex-none px-6 py-3 bg-primary-container text-on-primary-container rounded-xl font-title-md text-title-md soft-elevation hover:opacity-90 transition-all flex items-center justify-center gap-2"
                    >
                      <span className="material-symbols-outlined">input</span>
                      Registrar Ingreso
                    </button>
                    <button
                      onClick={() => handleAbrirMovimiento('Egreso')}
                      className="flex-1 md:flex-none px-6 py-3 bg-secondary-container text-on-secondary-container rounded-xl font-title-md text-title-md soft-elevation hover:opacity-90 transition-all flex items-center justify-center gap-2"
                    >
                      <span className="material-symbols-outlined">output</span>
                      Registrar Egreso
                    </button>
                  </div>
                )}
              </div>

              <section className="grid grid-cols-1 md:grid-cols-3 gap-gutter">
                <div className="bg-surface-container-lowest p-8 rounded-3xl soft-elevation border-l-4 border-primary">
                  <p className="text-secondary font-body-sm mb-2">Saldo inicial</p>
                  <h3 className="text-headline-lg font-headline-lg text-on-surface">${formatMonto(sesion.montoInicial)}</h3>
                </div>
                <div className="bg-surface-container-lowest p-8 rounded-3xl soft-elevation border-l-4 border-tertiary-container">
                  <p className="text-secondary font-body-sm mb-2">Ingresos registrados</p>
                  <h3 className="text-headline-lg font-headline-lg text-on-surface">${formatMonto(totalIngresos)}</h3>
                </div>
                <div className="bg-surface-container-lowest p-8 rounded-3xl soft-elevation border-l-4 border-green-500">
                  <p className="text-secondary font-body-sm mb-2">Efectivo esperado</p>
                  <h3 className="text-headline-lg font-headline-lg text-primary">${formatMonto(sesion.montoEsperadoEfectivo)}</h3>
                </div>
              </section>

              <div className="flex flex-col lg:flex-row gap-gutter">
                <div className="lg:flex-[2] bg-surface-container-lowest rounded-3xl soft-elevation overflow-hidden">
                  <div className="p-8 flex justify-between items-center border-b border-outline-variant/30">
                    <h2 className="font-title-md text-title-md">Movimientos de caja</h2>
                  </div>
                  <div className="overflow-x-auto">
                    <table className="w-full text-left">
                      <thead>
                        <tr className="bg-surface-container-low text-secondary font-label-md">
                          <th className="px-8 py-4">Hora</th>
                          <th className="px-6 py-4">Tipo</th>
                          <th className="px-6 py-4">Concepto</th>
                          <th className="px-6 py-4">Método</th>
                          <th className="px-6 py-4">Usuario</th>
                          <th className="px-6 py-4 text-right">Monto</th>
                        </tr>
                      </thead>
                      <tbody className="divide-y divide-outline-variant/20">
                        {sesion.movimientos.length === 0 ? (
                          <tr>
                            <td colSpan={6} className="px-8 py-8 text-center text-secondary font-body-sm">
                              Todavía no hay movimientos en esta sesión.
                            </td>
                          </tr>
                        ) : (
                          [...sesion.movimientos].reverse().map((m) => (
                            <tr key={m.id} className="hover:bg-surface-container transition-colors">
                              <td className="px-8 py-5 text-body-sm">{formatHora(m.fechaHora)}</td>
                              <td className="px-6 py-5">
                                <span
                                  className={`px-3 py-1 rounded-full text-[12px] font-semibold ${
                                    m.tipo === 'Ingreso' ? 'bg-green-100 text-green-700' : 'bg-red-100 text-red-700'
                                  }`}
                                >
                                  {m.tipo}
                                </span>
                                {m.movimientoOrigenId && (
                                  <span className="ml-2 px-2 py-0.5 rounded-full text-[10px] bg-surface-variant text-on-surface-variant">
                                    Reversa
                                  </span>
                                )}
                              </td>
                              <td className="px-6 py-5 text-on-surface font-medium">{m.concepto}</td>
                              <td className="px-6 py-5 text-secondary">
                                <div className="flex items-center gap-2">
                                  <span className="material-symbols-outlined text-sm">{iconoPorNombre(m.nombreMetodoPagoSnapshot)}</span>
                                  {m.nombreMetodoPagoSnapshot}
                                </div>
                              </td>
                              <td className="px-6 py-5 text-secondary">{m.usuarioNombre}</td>
                              <td
                                className={`px-6 py-5 text-right font-bold ${
                                  m.tipo === 'Ingreso' ? 'text-green-600' : 'text-red-600'
                                }`}
                              >
                                {m.tipo === 'Ingreso' ? '+' : '-'}${formatMonto(m.monto)}
                              </td>
                            </tr>
                          ))
                        )}
                      </tbody>
                    </table>
                  </div>
                  <div className="p-6 bg-surface-container-low text-center">
                    <button className="text-primary font-title-md hover:underline" onClick={() => setTabActivo('historial')}>
                      Ver historial de sesiones cerradas
                    </button>
                  </div>
                </div>

                <aside className="lg:flex-1 flex flex-col gap-gutter">
                  <div className="bg-surface-container-lowest p-8 rounded-3xl soft-elevation">
                    <h2 className="font-title-md text-title-md mb-6 flex items-center gap-2">
                      <span className="material-symbols-outlined text-primary">analytics</span>
                      Distribución de fondos
                    </h2>
                    <div className="space-y-6">
                      <div>
                        <div className="flex justify-between mb-2">
                          <span className="text-body-sm text-secondary">Efectivo</span>
                          <span className="text-body-sm font-bold">${formatMonto(totalEfectivo)}</span>
                        </div>
                        <div className="w-full bg-surface-container-highest rounded-full h-2">
                          <div className="bg-primary-container h-2 rounded-full" style={{ width: `${pctEfectivo}%` }} />
                        </div>
                      </div>
                      <div>
                        <div className="flex justify-between mb-2">
                          <span className="text-body-sm text-secondary">Otros medios</span>
                          <span className="text-body-sm font-bold">${formatMonto(totalOtrosMedios)}</span>
                        </div>
                        <div className="w-full bg-surface-container-highest rounded-full h-2">
                          <div className="bg-surface-tint h-2 rounded-full" style={{ width: `${pctOtros}%` }} />
                        </div>
                      </div>
                    </div>
                  </div>

                  {puedeGestionar && (
                    <div className="bg-surface-container-lowest p-8 rounded-3xl soft-elevation flex-grow flex flex-col justify-between">
                      <div>
                        <h2 className="font-title-md text-title-md mb-2">Operación del día</h2>
                        <p className="text-body-sm text-secondary mb-6">
                          Cerrá la caja para declarar el efectivo contado y generar el resumen.
                        </p>
                      </div>
                      <button
                        onClick={abrirCerrarModal}
                        className="w-full py-4 border-2 border-error text-error rounded-2xl font-headline-lg-mobile hover:bg-error hover:text-on-error transition-all duration-300 flex items-center justify-center gap-2"
                      >
                        <span className="material-symbols-outlined">lock</span>
                        Cerrar caja
                      </button>
                    </div>
                  )}
                </aside>
              </div>
            </div>
          )}
        </Tabs.Panel>

        <Tabs.Panel value="historial" pt="lg">
          <HistorialCajaTab />
        </Tabs.Panel>
      </Tabs>

      {sesion && (
        <>
          <RegistrarMovimientoModal
            opened={movimientoModalOpened}
            onClose={cerrarMovimientoModal}
            onMovimientoRegistrado={cargarSesion}
            tipoInicial={tipoMovimientoInicial}
          />
          <CerrarCajaModal
            opened={cerrarModalOpened}
            onClose={cerrarCerrarModal}
            sesion={sesion}
            onSesionCerrada={cargarSesion}
          />
        </>
      )}
    </div>
  );
}
