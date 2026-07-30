// src/pages/Cobros/HistorialCobrosPage.tsx
import { useEffect, useState } from 'react';
import { DatePickerInput } from '@mantine/dates';
import { IconChevronLeft, IconChevronRight, IconReceipt2 } from '@tabler/icons-react';
import { cobrosService } from '../../api/cobrosService';
import { SearchInput } from '../../components/SearchInput';
import { EmptyState } from '../../components/EmptyState';
import { PageSpinner } from '../../components/PageSpinner';
import { usePermission } from '../../hooks/usePermission';
import type { CobroListItem, HistorialCobros } from '../../types/Cobro';

const TAMANO_PAGINA = 10;

function formatMonto(valor: number) {
  return valor.toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}

function iconoMetodo(nombre: string) {
  const n = nombre.toLowerCase();
  if (n.includes('efectivo')) return 'payments';
  if (n.includes('tarjeta') || n.includes('credito') || n.includes('crédito')) return 'credit_card';
  if (n.includes('transferencia') || n.includes('banco')) return 'account_balance';
  return 'payments';
}

// DatePickerInput entrega/recibe strings "YYYY-MM-DD" (no Date) — los mantenemos
// como string de punta a punta. Convertirlos a Date acá dispararía el bug clásico
// de `new Date("YYYY-MM-DD")` (se interpreta como medianoche UTC) que corre el día
// mostrado hacia atrás en husos horarios negativos como Argentina (UTC-3).
function finDeDia(fecha: string): string {
  return `${fecha}T23:59:59.999`;
}

export function HistorialCobrosPage() {
  const puedeVerGananciaNeta = usePermission('VerGananciaNeta');
  const [historial, setHistorial] = useState<HistorialCobros | null>(null);
  const [loading, setLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const [busquedaInput, setBusquedaInput] = useState('');
  const [busqueda, setBusqueda] = useState('');
  const [fechaDesde, setFechaDesde] = useState<string | null>(null);
  const [fechaHasta, setFechaHasta] = useState<string | null>(null);
  const [pagina, setPagina] = useState(1);

  // Debounce del texto de búsqueda antes de disparar el fetch.
  useEffect(() => {
    const timeout = setTimeout(() => {
      setBusqueda(busquedaInput);
      setPagina(1);
    }, 400);
    return () => clearTimeout(timeout);
  }, [busquedaInput]);

  useEffect(() => {
    let cancelado = false;

    async function cargar() {
      setLoading(true);
      try {
        const data = await cobrosService.getHistorial({
          busqueda: busqueda || undefined,
          fechaDesde: fechaDesde ?? undefined,
          fechaHasta: fechaHasta ? finDeDia(fechaHasta) : undefined,
          pagina,
          tamanoPagina: TAMANO_PAGINA,
        });
        if (!cancelado) {
          setHistorial(data);
          setErrorMessage(null);
        }
      } catch {
        if (!cancelado) setErrorMessage('No pudimos cargar el historial de cobros.');
      } finally {
        if (!cancelado) setLoading(false);
      }
    }

    cargar();
    return () => {
      cancelado = true;
    };
  }, [busqueda, fechaDesde, fechaHasta, pagina]);

  const totalPages = historial ? Math.max(1, Math.ceil(historial.totalCount / historial.tamanoPagina)) : 1;
  const desde = historial && historial.totalCount > 0 ? (historial.pagina - 1) * historial.tamanoPagina + 1 : 0;
  const hasta = historial ? Math.min(historial.pagina * historial.tamanoPagina, historial.totalCount) : 0;

  return (
    <div className="flex flex-col gap-12 pb-12">
      <div className="flex flex-col md:flex-row justify-between items-start md:items-end gap-6">
        <div>
          <h1 className="font-headline-lg text-headline-lg text-on-background">Historial de Cobros</h1>
          <p className="font-body-lg text-body-lg text-secondary mt-2">Visión financiera consolidada de tus turnos.</p>
        </div>
        <div data-tour="cobros-filtros" className="flex flex-wrap gap-4 w-full md:w-auto">
          <SearchInput
            className="flex-grow md:w-64"
            placeholder="Buscar cliente o turno..."
            value={busquedaInput}
            onSearch={setBusquedaInput}
          />
          <DatePickerInput
            placeholder="Desde"
            value={fechaDesde}
            onChange={(value) => {
              setFechaDesde(value);
              setPagina(1);
            }}
            clearable
            className="w-40"
          />
          <DatePickerInput
            placeholder="Hasta"
            value={fechaHasta}
            onChange={(value) => {
              setFechaHasta(value);
              setPagina(1);
            }}
            clearable
            className="w-40"
          />
        </div>
      </div>

      {errorMessage && (
        <div
          role="alert"
          className="flex items-center gap-2 rounded-lg border border-error/30 bg-error-container px-4 py-3 font-body-sm text-body-sm text-on-error-container"
        >
          <span className="material-symbols-outlined text-[20px]">error</span>
          {errorMessage}
        </div>
      )}

      <section className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <div className="bg-surface-container-lowest rounded-3xl p-6 soft-elevation border border-surface-variant flex flex-col gap-2 relative overflow-hidden">
          <div className="flex justify-between items-center">
            <span className="font-body-sm text-body-sm text-secondary">Total Cobrado (período)</span>
            <span className="material-symbols-outlined text-primary bg-primary/10 p-2 rounded-full">
              account_balance_wallet
            </span>
          </div>
          <div className="font-display-lg text-display-lg text-on-background">
            ${historial ? formatMonto(historial.totalCobradoPeriodo) : '—'}
          </div>
        </div>

        <div className="bg-surface-container-lowest rounded-3xl p-6 soft-elevation border border-surface-variant flex flex-col gap-2 relative overflow-hidden">
          <div className="flex justify-between items-center">
            <span className="font-body-sm text-body-sm text-secondary">Saldo Pendiente (global)</span>
            <span className="material-symbols-outlined text-error bg-error/10 p-2 rounded-full">pending_actions</span>
          </div>
          <div className="font-display-lg text-display-lg text-on-background">
            ${historial ? formatMonto(historial.saldoPendienteGlobal) : '—'}
          </div>
        </div>

        {puedeVerGananciaNeta && (
          <div className="bg-surface-container-lowest rounded-3xl p-6 soft-elevation border border-surface-variant flex flex-col gap-2 relative overflow-hidden">
            <div className="flex justify-between items-center">
              <span className="font-body-sm text-body-sm text-secondary">Comisiones Totales (período)</span>
              <span className="material-symbols-outlined text-tertiary bg-tertiary/10 p-2 rounded-full">receipt_long</span>
            </div>
            <div className="font-display-lg text-display-lg text-on-background">
              ${historial ? formatMonto(historial.comisionesTotalesPeriodo ?? 0) : '—'}
            </div>
          </div>
        )}
      </section>

      <section className="bg-surface-container-lowest rounded-3xl soft-elevation overflow-hidden border border-surface-variant">
        {loading ? (
          <PageSpinner />
        ) : !historial || historial.items.length === 0 ? (
          <EmptyState
            title="Sin cobros en este período"
            description="Todavía no hay cobros registrados con estos filtros. Registrá un cobro desde el detalle de un turno."
            icon={IconReceipt2}
            actionLabel="Limpiar filtros"
            onAction={() => {
              setBusquedaInput('');
              setFechaDesde(null);
              setFechaHasta(null);
              setPagina(1);
            }}
          />
        ) : (
          <>
            <div className="overflow-x-auto">
              <table className="w-full text-left border-collapse">
                <thead>
                  <tr className="bg-surface-container-low border-b border-outline-variant">
                    <th className="py-4 px-6 font-label-md text-label-md text-secondary">Cliente / Turno</th>
                    <th className="py-4 px-6 font-label-md text-label-md text-secondary">Fecha</th>
                    <th className="py-4 px-6 font-label-md text-label-md text-secondary">Método</th>
                    <th className="py-4 px-6 font-label-md text-label-md text-secondary text-right">Precio Base</th>
                    <th className="py-4 px-6 font-label-md text-label-md text-secondary text-right">Modificador</th>
                    <th className="py-4 px-6 font-label-md text-label-md text-secondary text-right">Precio Final</th>
                    {puedeVerGananciaNeta && (
                      <>
                        <th className="py-4 px-6 font-label-md text-label-md text-secondary text-right">Comisión</th>
                        <th className="py-4 px-6 font-label-md text-label-md text-secondary text-right">Ganancia</th>
                      </>
                    )}
                  </tr>
                </thead>
                <tbody className="divide-y divide-surface-variant">
                  {historial.items.map((item: CobroListItem) => (
                    <tr key={item.id} className="hover:bg-surface-container transition-colors">
                      <td className="py-4 px-6">
                        <div className="font-title-md text-body-sm text-on-surface">{item.clienteNombreCompleto}</div>
                        <div className="font-body-sm text-label-md text-secondary">
                          T-{item.turnoId} • {item.serviciosResumen}
                        </div>
                      </td>
                      <td className="py-4 px-6 font-body-sm text-body-sm text-on-surface-variant">
                        {new Date(item.fechaHoraTurno).toLocaleString('es-AR', {
                          day: '2-digit',
                          month: 'short',
                          hour: '2-digit',
                          minute: '2-digit',
                        })}
                      </td>
                      <td className="py-4 px-6 font-body-sm text-body-sm text-on-surface-variant">
                        <div className="flex items-center gap-2">
                          <span className="material-symbols-outlined text-[18px]">
                            {iconoMetodo(item.nombreMetodoPagoSnapshot)}
                          </span>
                          {item.nombreMetodoPagoSnapshot}
                        </div>
                      </td>
                      <td className="py-4 px-6 font-body-sm text-body-sm text-on-surface text-right">
                        ${formatMonto(item.precioBase)}
                      </td>
                      <td className="py-4 px-6 font-body-sm text-body-sm text-secondary text-right">
                        {item.montoModificadorCliente === 0
                          ? '-'
                          : `${item.montoModificadorCliente > 0 ? '+' : ''}$${formatMonto(item.montoModificadorCliente)}`}
                      </td>
                      <td className="py-4 px-6 font-title-md text-body-sm text-on-background text-right">
                        ${formatMonto(item.precioFinal)}
                      </td>
                      {puedeVerGananciaNeta && (
                        <>
                          <td className="py-4 px-6 font-body-sm text-body-sm text-tertiary text-right">
                            -${formatMonto(item.montoComision ?? 0)}
                          </td>
                          <td className="py-4 px-6 font-title-md text-body-sm text-primary text-right">
                            ${formatMonto(item.gananciaNeta ?? 0)}
                          </td>
                        </>
                      )}
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            <div className="px-6 py-4 border-t border-surface-variant flex justify-between items-center bg-surface-container-lowest">
              <span className="font-body-sm text-body-sm text-secondary">
                Mostrando {desde}-{hasta} de {historial.totalCount} cobros
              </span>
              <div className="flex items-center gap-2">
                <button
                  disabled={historial.pagina <= 1}
                  onClick={() => setPagina((p) => Math.max(1, p - 1))}
                  className="w-8 h-8 rounded flex items-center justify-center border border-outline-variant text-secondary hover:bg-surface-container-low transition-colors disabled:opacity-40 disabled:cursor-not-allowed"
                >
                  <IconChevronLeft size={18} />
                </button>
                <span className="font-body-sm text-body-sm text-on-surface px-2">
                  Página {historial.pagina} de {totalPages}
                </span>
                <button
                  disabled={historial.pagina >= totalPages}
                  onClick={() => setPagina((p) => Math.min(totalPages, p + 1))}
                  className="w-8 h-8 rounded flex items-center justify-center border border-outline-variant text-secondary hover:bg-surface-container-low transition-colors disabled:opacity-40 disabled:cursor-not-allowed"
                >
                  <IconChevronRight size={18} />
                </button>
              </div>
            </div>
          </>
        )}
      </section>
    </div>
  );
}
