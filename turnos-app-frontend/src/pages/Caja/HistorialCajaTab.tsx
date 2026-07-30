// src/pages/Caja/HistorialCajaTab.tsx
import { useEffect, useState } from 'react';
import { DatePickerInput } from '@mantine/dates';
import { IconChevronLeft, IconChevronRight, IconCash } from '@tabler/icons-react';
import { useDisclosure } from '@mantine/hooks';
import { cajaService } from '../../api/cajaService';
import { EmptyState } from '../../components/EmptyState';
import { PageSpinner } from '../../components/PageSpinner';
import { DetalleSesionModal } from './DetalleSesionModal';
import type { HistorialSesionesCaja, SesionCajaListItem } from '../../types/Caja';

const TAMANO_PAGINA = 10;

function formatMonto(valor: number) {
  return valor.toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}

function formatFecha(iso: string) {
  return new Date(iso).toLocaleString('es-AR', { day: '2-digit', month: 'short', hour: '2-digit', minute: '2-digit' });
}

// Mismo motivo que HistorialCobrosPage.tsx: DatePickerInput entrega/recibe "YYYY-MM-DD" como
// string plano — convertirlo a Date acá dispara el bug de `new Date("YYYY-MM-DD")` (UTC medianoche).
function finDeDia(fecha: string): string {
  return `${fecha}T23:59:59.999`;
}

export function HistorialCajaTab() {
  const [historial, setHistorial] = useState<HistorialSesionesCaja | null>(null);
  const [loading, setLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [fechaDesde, setFechaDesde] = useState<string | null>(null);
  const [fechaHasta, setFechaHasta] = useState<string | null>(null);
  const [pagina, setPagina] = useState(1);
  const [sesionSeleccionada, setSesionSeleccionada] = useState<SesionCajaListItem | null>(null);
  const [detalleOpened, { open: abrirDetalle, close: cerrarDetalle }] = useDisclosure(false);

  useEffect(() => {
    let cancelado = false;

    async function cargar() {
      setLoading(true);
      try {
        const data = await cajaService.getHistorial({
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
        if (!cancelado) setErrorMessage('No pudimos cargar el historial de caja.');
      } finally {
        if (!cancelado) setLoading(false);
      }
    }

    cargar();
    return () => {
      cancelado = true;
    };
  }, [fechaDesde, fechaHasta, pagina]);

  const totalPages = historial ? Math.max(1, Math.ceil(historial.totalCount / historial.tamanoPagina)) : 1;

  const handleVerDetalle = (sesion: SesionCajaListItem) => {
    setSesionSeleccionada(sesion);
    abrirDetalle();
  };

  return (
    <div className="flex flex-col gap-6">
      <div className="flex flex-wrap gap-4">
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

      {errorMessage && (
        <div
          role="alert"
          className="flex items-center gap-2 rounded-lg border border-error/30 bg-error-container px-4 py-3 font-body-sm text-body-sm text-on-error-container"
        >
          <span className="material-symbols-outlined text-[20px]">error</span>
          {errorMessage}
        </div>
      )}

      <section className="bg-surface-container-lowest rounded-3xl soft-elevation overflow-hidden border border-surface-variant">
        {loading ? (
          <PageSpinner />
        ) : !historial || historial.items.length === 0 ? (
          <EmptyState
            title="Sin sesiones de caja"
            description="Todavía no hay sesiones de caja cerradas en este período."
            icon={IconCash}
            actionLabel="Limpiar filtros"
            onAction={() => {
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
                    <th className="py-4 px-6 font-label-md text-label-md text-secondary">Apertura</th>
                    <th className="py-4 px-6 font-label-md text-label-md text-secondary">Cierre</th>
                    <th className="py-4 px-6 font-label-md text-label-md text-secondary">Usuario</th>
                    <th className="py-4 px-6 font-label-md text-label-md text-secondary text-right">Inicial</th>
                    <th className="py-4 px-6 font-label-md text-label-md text-secondary text-right">Esperado</th>
                    <th className="py-4 px-6 font-label-md text-label-md text-secondary text-right">Declarado</th>
                    <th className="py-4 px-6 font-label-md text-label-md text-secondary text-right">Diferencia</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-surface-variant">
                  {historial.items.map((s) => (
                    <tr
                      key={s.id}
                      className="hover:bg-surface-container transition-colors cursor-pointer"
                      onClick={() => handleVerDetalle(s)}
                    >
                      <td className="py-4 px-6 font-body-sm text-body-sm text-on-surface-variant">
                        {formatFecha(s.fechaApertura)}
                      </td>
                      <td className="py-4 px-6 font-body-sm text-body-sm text-on-surface-variant">
                        <div className="flex items-center gap-2">
                          {s.fechaCierre ? formatFecha(s.fechaCierre) : '—'}
                          {s.cierreForzado && (
                            <span className="inline-flex items-center px-2 py-0.5 rounded-full bg-tertiary-container text-on-tertiary-container font-label-md text-[10px]">
                              Forzado
                            </span>
                          )}
                        </div>
                      </td>
                      <td className="py-4 px-6 font-body-sm text-body-sm text-on-surface-variant">
                        {s.usuarioCierreNombre && s.usuarioCierreNombre !== s.usuarioAperturaNombre
                          ? `${s.usuarioAperturaNombre} → ${s.usuarioCierreNombre}`
                          : s.usuarioAperturaNombre}
                      </td>
                      <td className="py-4 px-6 font-body-sm text-body-sm text-on-surface text-right">
                        ${formatMonto(s.montoInicial)}
                      </td>
                      <td className="py-4 px-6 font-body-sm text-body-sm text-on-surface text-right">
                        ${formatMonto(s.montoEsperadoEfectivo)}
                      </td>
                      <td className="py-4 px-6 font-body-sm text-body-sm text-on-surface text-right">
                        {s.montoFinalDeclarado !== null ? `$${formatMonto(s.montoFinalDeclarado)}` : '—'}
                      </td>
                      <td className="py-4 px-6 font-title-md text-body-sm text-right">
                        {s.diferencia === null ? (
                          '—'
                        ) : (
                          <span className={s.diferencia === 0 ? 'text-secondary' : s.diferencia > 0 ? 'text-tertiary-container' : 'text-error'}>
                            {s.diferencia === 0 ? 'Sin diferencia' : `${s.diferencia > 0 ? '+' : ''}$${formatMonto(s.diferencia)}`}
                          </span>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            <div className="px-6 py-4 border-t border-surface-variant flex justify-between items-center bg-surface-container-lowest">
              <span className="font-body-sm text-body-sm text-secondary">
                Página {historial.pagina} de {totalPages} · {historial.totalCount} sesiones
              </span>
              <div className="flex items-center gap-2">
                <button
                  disabled={historial.pagina <= 1}
                  onClick={() => setPagina((p) => Math.max(1, p - 1))}
                  className="w-8 h-8 rounded flex items-center justify-center border border-outline-variant text-secondary hover:bg-surface-container-low transition-colors disabled:opacity-40 disabled:cursor-not-allowed"
                >
                  <IconChevronLeft size={18} />
                </button>
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

      {sesionSeleccionada && (
        <DetalleSesionModal opened={detalleOpened} onClose={cerrarDetalle} sesionId={sesionSeleccionada.id} />
      )}
    </div>
  );
}
