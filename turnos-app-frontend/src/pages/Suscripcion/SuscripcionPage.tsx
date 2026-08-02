// src/pages/Suscripcion/SuscripcionPage.tsx
import { useEffect, useState } from 'react';
import axios from 'axios';
import { usePermission } from '../../hooks/usePermission';
import { PageSpinner } from '../../components/PageSpinner';
import { formatMonto } from '../../utils/format';
import { suscripcionService } from '../../api/suscripcionService';
import type { Suscripcion, EstadoSuscripcion } from '../../types/Suscripcion';

const LABEL_POR_ESTADO: Record<EstadoSuscripcion, string> = {
  Trial: 'Período de prueba',
  Activa: 'Activa',
  PastDue: 'Pago pendiente',
  Cancelada: 'Cancelada',
};

const BADGE_POR_ESTADO: Record<EstadoSuscripcion, string> = {
  Trial: 'bg-primary/10 text-primary',
  Activa: 'bg-primary text-on-primary',
  PastDue: 'bg-tertiary/10 text-tertiary',
  Cancelada: 'bg-error/10 text-error',
};

const ICONO_POR_ESTADO: Record<EstadoSuscripcion, string> = {
  Trial: 'schedule',
  Activa: 'check_circle',
  PastDue: 'warning',
  Cancelada: 'error',
};

export function SuscripcionPage() {
  const puedeGestionar = usePermission('GestionarSuscripcion');

  const [suscripcion, setSuscripcion] = useState<Suscripcion | null>(null);
  const [loading, setLoading] = useState(true);
  const [procesando, setProcesando] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const cargarEstado = async () => {
    try {
      const data = await suscripcionService.getEstado();
      setSuscripcion(data);
      setErrorMessage(null);
    } catch {
      setErrorMessage('No pudimos cargar el estado de tu suscripción.');
    }
  };

  useEffect(() => {
    cargarEstado().finally(() => setLoading(false));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const manejarError = (error: unknown, fallback: string) => {
    if (axios.isAxiosError(error) && error.response?.data?.detail) {
      setErrorMessage(error.response.data.detail);
    } else {
      setErrorMessage(fallback);
    }
  };

  const iniciarSuscripcion = async () => {
    setProcesando(true);
    setErrorMessage(null);
    try {
      const url = await suscripcionService.iniciar();
      window.location.href = url;
    } catch (error) {
      manejarError(error, 'No pudimos iniciar la suscripción con Mercado Pago. Intentá de nuevo.');
      setProcesando(false);
    }
  };

  const cancelarSuscripcion = async () => {
    if (!window.confirm('¿Seguro que querés cancelar la suscripción?')) return;

    setProcesando(true);
    setErrorMessage(null);
    try {
      await suscripcionService.cancelar();
      await cargarEstado();
    } catch (error) {
      manejarError(error, 'No pudimos cancelar la suscripción. Intentá de nuevo.');
    } finally {
      setProcesando(false);
    }
  };

  if (loading) {
    return <PageSpinner />;
  }

  const mensajePorEstado = (estado: EstadoSuscripcion): string => {
    switch (estado) {
      case 'Trial':
        return suscripcion?.suscripcionVenceEn
          ? `Tu período de prueba vence el ${new Date(suscripcion.suscripcionVenceEn).toLocaleDateString('es-AR')}.`
          : 'Estás en período de prueba con acceso completo a Turnify.';
      case 'Activa':
        return 'Tu suscripción está activa y al día.';
      case 'PastDue':
        return suscripcion?.diasRestantesGracia !== null && suscripcion?.diasRestantesGracia !== undefined
          ? suscripcion.diasRestantesGracia > 0
            ? `No pudimos procesar tu pago. Tenés ${suscripcion.diasRestantesGracia} ${suscripcion.diasRestantesGracia === 1 ? 'día' : 'días'} para actualizar tu método de pago antes de perder el acceso.`
            : 'No pudimos procesar tu pago y el período de gracia terminó. Actualizá tu método de pago para recuperar el acceso.'
          : 'Tu suscripción está pausada por Mercado Pago. Iniciá una suscripción nueva para reactivar el acceso.';
      case 'Cancelada':
        return 'Tu suscripción está cancelada. Suscribite de nuevo para seguir usando Turnify.';
    }
  };

  return (
    <div className="flex flex-col gap-10 pb-12 max-w-4xl">
      <header>
        <h1 className="font-headline-lg text-headline-lg text-on-surface">Suscripción</h1>
        <p className="font-body-lg text-body-lg text-secondary mt-2">
          Estado de la suscripción de tu negocio a Turnify.
        </p>
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

      {suscripcion && (
        <div className="bg-surface-container-lowest rounded-3xl p-8 md:p-12 soft-elevation flex flex-col gap-8">
          <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-6">
            <div className="flex flex-col gap-4 flex-grow">
              <div className="flex items-center gap-3">
                <span
                  className="material-symbols-outlined text-primary text-3xl"
                  style={{ fontVariationSettings: "'FILL' 1" }}
                >
                  workspace_premium
                </span>
                <h2 className="font-headline-lg text-headline-lg text-on-surface">
                  {suscripcion.planNombre ?? 'Sin plan asignado'}
                </h2>
              </div>

              {suscripcion.planPrecioMensual != null && !suscripcion.esGrandfathered && (
                <p className="font-headline-lg text-headline-lg text-primary">
                  ARS ${formatMonto(suscripcion.planPrecioMensual)}{' '}
                  <span className="font-body-lg text-body-lg font-normal text-secondary">/ mes</span>
                </p>
              )}

              <div className="flex items-center gap-2 text-secondary">
                <span className="material-symbols-outlined text-[18px]">
                  {suscripcion.esGrandfathered ? 'info' : ICONO_POR_ESTADO[suscripcion.estadoSuscripcion]}
                </span>
                <p className="font-body-lg text-body-lg">
                  {suscripcion.esGrandfathered
                    ? 'Tu cuenta tiene acceso completo sin cargo, sin necesidad de suscribirte.'
                    : mensajePorEstado(suscripcion.estadoSuscripcion)}
                </p>
              </div>
            </div>

            <span
              className={`flex-shrink-0 inline-flex items-center px-4 py-2 rounded-full font-label-md text-label-md tracking-wider ${
                suscripcion.esGrandfathered ? 'bg-[#9c27b0]/10 text-[#9c27b0]' : BADGE_POR_ESTADO[suscripcion.estadoSuscripcion]
              }`}
            >
              {suscripcion.esGrandfathered ? 'ACCESO ILIMITADO' : LABEL_POR_ESTADO[suscripcion.estadoSuscripcion].toUpperCase()}
            </span>
          </div>

          <div className="h-1.5 w-full bg-surface-container rounded-full overflow-hidden">
            <div className="h-full bg-primary w-full opacity-20" />
          </div>

          {!suscripcion.esGrandfathered && (
            <>
              {puedeGestionar ? (
                <div className="flex">
                  {suscripcion.estadoSuscripcion === 'Activa' ? (
                    <button
                      onClick={cancelarSuscripcion}
                      disabled={procesando}
                      className="flex items-center justify-center gap-2 px-6 py-3 rounded-full border border-error text-error font-label-md text-label-md hover:bg-error-container transition-colors disabled:opacity-60 disabled:cursor-not-allowed"
                    >
                      {procesando ? (
                        <span className="material-symbols-outlined animate-spin text-[18px]">progress_activity</span>
                      ) : (
                        <span className="material-symbols-outlined text-[18px]">cancel</span>
                      )}
                      Cancelar suscripción
                    </button>
                  ) : (
                    <button
                      onClick={iniciarSuscripcion}
                      disabled={procesando}
                      className="flex items-center justify-center gap-2 bg-primary text-on-primary font-label-md text-label-md px-6 py-3 rounded-full soft-elevation hover:bg-primary-container hover:shadow-lg transition-all duration-200 disabled:opacity-60 disabled:cursor-not-allowed"
                    >
                      {procesando ? (
                        <span className="material-symbols-outlined animate-spin text-[18px]">progress_activity</span>
                      ) : (
                        <span className="material-symbols-outlined text-[18px]">open_in_new</span>
                      )}
                      Suscribirme con Mercado Pago
                    </button>
                  )}
                </div>
              ) : (
                <p className="font-body-sm text-body-sm text-secondary">
                  Necesitás el permiso "Gestionar suscripción" para administrar el pago del negocio.
                </p>
              )}
            </>
          )}
        </div>
      )}

      <div className="grid grid-cols-1 md:grid-cols-2 gap-gutter">
        <div className="bg-surface-container-low rounded-2xl p-6 flex flex-col gap-4 border border-outline-variant/10">
          <span className="material-symbols-outlined text-primary p-3 bg-surface-container-lowest rounded-xl shadow-sm w-fit">
            auto_graph
          </span>
          <h3 className="font-title-md text-title-md text-on-surface">Crecimiento sin límites</h3>
          <p className="font-body-sm text-body-sm text-secondary">
            Disfrutá de todas las funcionalidades de gestión de turnos, clientes y reportes financieros sin
            restricciones de volumen.
          </p>
        </div>
        <div className="bg-surface-container-low rounded-2xl p-6 flex flex-col gap-4 border border-outline-variant/10">
          <span className="material-symbols-outlined text-primary p-3 bg-surface-container-lowest rounded-xl shadow-sm w-fit">
            support_agent
          </span>
          <h3 className="font-title-md text-title-md text-on-surface">Soporte prioritario</h3>
          <p className="font-body-sm text-body-sm text-secondary">
            Nuestro equipo está disponible para ayudarte a configurar tu agenda y optimizar tus tiempos.
          </p>
        </div>
      </div>
    </div>
  );
}
