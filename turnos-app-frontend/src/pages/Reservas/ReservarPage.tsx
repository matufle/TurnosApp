// src/pages/Reservas/ReservarPage.tsx
// Detrás de ClienteProtectedRoute: reserva obligatoria, como pide el enunciado original.
// El servicio ?servicioId= de la URL (elegido en CatalogoPage) es solo el punto de partida:
// el cliente puede sumar más servicios al mismo turno acá, en paridad con el flujo de staff
// en TurnosPage.tsx (que ya soportaba multi-servicio desde el principio).
import { useEffect, useMemo, useState } from 'react';
import { useNavigate, useOutletContext, useParams, useSearchParams } from 'react-router-dom';
import { Alert, Button, Center, Loader, MultiSelect, Select, Stack, Text } from '@mantine/core';
import { DatePicker, DatesProvider } from '@mantine/dates';
import 'dayjs/locale/es';
import axios from 'axios';
import { publicCatalogoService } from '../../api/publicCatalogoService';
import { misTurnosService } from '../../api/misTurnosService';
import { trackEvent } from '../../lib/analytics';
import { horaUtcALocal } from '../../utils/horarioTimezone';
import type { Servicio } from '../../types/Servicio';
import type { RecursoPublico } from '../../types/ReservaPublica';
import type { TenantPublico } from '../../types/ClienteAuth';

export function ReservarPage() {
  const { slug } = useParams<{ slug: string }>();
  const { tenant } = useOutletContext<{ tenant: TenantPublico }>();
  const [searchParams] = useSearchParams();
  const servicioIdInicial = Number(searchParams.get('servicioId'));
  const navigate = useNavigate();

  const [servicios, setServicios] = useState<Servicio[]>([]);
  const [servicioIdsSeleccionados, setServicioIdsSeleccionados] = useState<string[]>([]);
  const [recursos, setRecursos] = useState<RecursoPublico[]>([]);
  const [recursoId, setRecursoId] = useState<string | null>(null);
  const [fecha, setFecha] = useState<string | null>(null);
  const [slotsUtc, setSlotsUtc] = useState<string[]>([]);
  const [slotSeleccionado, setSlotSeleccionado] = useState<string | null>(null);
  const [cargandoSlots, setCargandoSlots] = useState(false);
  const [cargandoInicial, setCargandoInicial] = useState(true);
  const [confirmando, setConfirmando] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [errorCarga, setErrorCarga] = useState(false);

  useEffect(() => {
    if (!slug || !servicioIdInicial) return;

    setCargandoInicial(true);
    setErrorCarga(false);

    Promise.all([publicCatalogoService.getServicios(slug), publicCatalogoService.getRecursos(slug)])
      .then(([serviciosData, recursosData]) => {
        setServicios(serviciosData);
        setServicioIdsSeleccionados(
          serviciosData.some((s) => s.id === servicioIdInicial) ? [String(servicioIdInicial)] : []
        );
        setRecursos(recursosData);
        if (recursosData.length > 0) setRecursoId(String(recursosData[0].id));
      })
      .catch(() => setErrorCarga(true))
      .finally(() => setCargandoInicial(false));
  }, [slug, servicioIdInicial]);

  const servicioIds = useMemo(() => servicioIdsSeleccionados.map(Number), [servicioIdsSeleccionados]);

  const { duracionTotal, precioTotal } = useMemo(() => {
    const elegidos = servicios.filter((s) => servicioIds.includes(s.id));
    return {
      duracionTotal: elegidos.reduce((acc, s) => acc + s.duracionMinutos, 0),
      precioTotal: elegidos.reduce((acc, s) => acc + s.precio, 0),
    };
  }, [servicios, servicioIds]);

  useEffect(() => {
    setSlotSeleccionado(null);

    if (!slug || !recursoId || !fecha || servicioIds.length === 0) {
      setSlotsUtc([]);
      return;
    }

    setCargandoSlots(true);
    publicCatalogoService
      .getDisponibilidad(slug, Number(recursoId), servicioIds, fecha)
      .then(setSlotsUtc)
      .finally(() => setCargandoSlots(false));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [slug, recursoId, fecha, servicioIds.join(',')]);

  const confirmar = async () => {
    if (!slug || !recursoId || !fecha || !slotSeleccionado || servicioIds.length === 0) return;

    setErrorMessage(null);
    setConfirmando(true);

    try {
      // slotSeleccionado ya viene en la misma convención "UTC-equivalente" que usa el backend —
      // combinarlo directo con la fecha arma el instante UTC correcto (ver horarioTimezone.ts).
      await misTurnosService.crear({
        recursoId: Number(recursoId),
        servicioIds,
        fechaHoraInicio: `${fecha}T${slotSeleccionado}:00.000Z`,
      });
      trackEvent('Appointment Created', { origen: 'self-service' });
      navigate(`/reservas/${slug}/mis-turnos`, { replace: true });
    } catch (error) {
      if (axios.isAxiosError(error) && error.response?.status === 409) {
        setErrorMessage('Ese horario ya no está disponible. Elegí otro.');
        setSlotsUtc((prev) => prev.filter((s) => s !== slotSeleccionado));
        setSlotSeleccionado(null);
      } else {
        setErrorMessage('No pudimos confirmar la reserva. Intentá de nuevo.');
      }
    } finally {
      setConfirmando(false);
    }
  };

  if (!servicioIdInicial) {
    return (
      <Center py="xl">
        <Text c="dimmed">Elegí un servicio desde el catálogo para reservar.</Text>
      </Center>
    );
  }

  if (cargandoInicial) {
    return (
      <Center py="xl">
        <Loader type="dots" />
      </Center>
    );
  }

  if (errorCarga) {
    return (
      <Center py="xl">
        <Stack align="center" gap="sm">
          <Text c="dimmed">No pudimos cargar los datos de reserva. Intentá de nuevo.</Text>
          <Button variant="light" onClick={() => window.location.reload()}>
            Reintentar
          </Button>
        </Stack>
      </Center>
    );
  }

  const nombresServiciosElegidos = servicios
    .filter((s) => servicioIds.includes(s.id))
    .map((s) => s.nombre)
    .join(', ');

  const fechaFormateada = fecha
    ? new Date(`${fecha}T00:00:00`).toLocaleDateString('es-AR', { weekday: 'long', day: 'numeric', month: 'short' })
    : null;

  const puedeConfirmar = Boolean(recursoId && fecha && slotSeleccionado && servicioIds.length > 0);

  return (
    <div className="w-full">
      <div className="max-w-4xl mx-auto px-margin-mobile md:px-margin-desktop pt-8 md:pt-16 pb-6 text-center md:text-left">
        <h2 className="font-headline-lg-mobile md:font-headline-lg text-on-surface mb-2">Reservar turno</h2>
        <p className="text-on-surface-variant font-body-lg">
          Completá los pasos para agendar tu turno con {tenant.nombre}
        </p>
      </div>

      <div className="max-w-4xl mx-auto px-margin-mobile md:px-margin-desktop pb-16">
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-gutter">
          {/* Columna del formulario */}
          <div className="lg:col-span-2 space-y-6">
            {/* Servicios */}
            <section className="bg-surface-container-lowest p-6 md:p-8 rounded-3xl soft-elevation border border-outline-variant/20">
              <div className="flex items-center gap-3 mb-6">
                <span className="material-symbols-outlined text-primary">content_cut</span>
                <h3 className="font-title-md text-on-surface">Servicios</h3>
              </div>
              <Stack gap="md">
                <MultiSelect
                  placeholder="Elegí uno o más servicios"
                  radius="md"
                  data={servicios.map((s) => ({
                    value: String(s.id),
                    label: `${s.nombre} (${s.duracionMinutos} min · $${s.precio})`,
                  }))}
                  value={servicioIdsSeleccionados}
                  onChange={setServicioIdsSeleccionados}
                />
                {servicioIds.length > 0 && (
                  <div className="flex items-center justify-between p-4 bg-surface-container-low rounded-2xl border border-dashed border-outline-variant">
                    <div className="flex flex-col">
                      <span className="text-on-surface-variant font-label-md text-label-md">Resumen</span>
                      <p className="text-on-surface font-title-md font-semibold">
                        Duración total: {duracionTotal} min · Precio total: ${precioTotal}
                      </p>
                    </div>
                  </div>
                )}
              </Stack>
            </section>

            {/* Con quién: solo si hay más de un recurso — con 1 solo se auto-selecciona */}
            {recursos.length > 1 && (
              <section className="bg-surface-container-lowest p-6 md:p-8 rounded-3xl soft-elevation border border-outline-variant/20">
                <div className="flex items-center gap-3 mb-6">
                  <span className="material-symbols-outlined text-primary">person</span>
                  <h3 className="font-title-md text-on-surface">Con quién</h3>
                </div>
                <Select
                  placeholder="Elegí con quién"
                  radius="md"
                  data={recursos.map((r) => ({ value: String(r.id), label: r.nombre }))}
                  value={recursoId}
                  onChange={setRecursoId}
                />
              </section>
            )}

            {/* Fecha */}
            <section className="bg-surface-container-lowest p-6 md:p-8 rounded-3xl soft-elevation border border-outline-variant/20">
              <div className="flex items-center gap-3 mb-6">
                <span className="material-symbols-outlined text-primary">calendar_month</span>
                <h3 className="font-title-md text-on-surface">Fecha</h3>
              </div>
              <DatesProvider settings={{ locale: 'es', firstDayOfWeek: 1 }}>
                <Center>
                  <DatePicker value={fecha} onChange={setFecha} minDate={new Date()} size="md" />
                </Center>
              </DatesProvider>
            </section>

            {/* Horarios */}
            <section className="bg-surface-container-lowest p-6 md:p-8 rounded-3xl soft-elevation border border-outline-variant/20">
              <div className="flex items-center gap-3 mb-6">
                <span className="material-symbols-outlined text-primary">schedule</span>
                <h3 className="font-title-md text-on-surface">Horarios disponibles</h3>
              </div>

              {errorMessage && (
                <Alert color="red" mb="md">
                  {errorMessage}
                </Alert>
              )}

              {servicioIds.length === 0 && (
                <p className="text-on-surface-variant font-body-sm">
                  Elegí al menos un servicio para ver los horarios disponibles.
                </p>
              )}

              {cargandoSlots && (
                <Center py="md">
                  <Loader type="dots" />
                </Center>
              )}

              {!cargandoSlots && recursoId && fecha && servicioIds.length > 0 && slotsUtc.length === 0 && (
                <p className="text-on-surface-variant font-body-sm">No hay horarios disponibles ese día.</p>
              )}

              {!cargandoSlots && slotsUtc.length > 0 && (
                <div className="grid grid-cols-3 sm:grid-cols-4 md:grid-cols-5 gap-3">
                  {slotsUtc.map((slotUtc) => {
                    const seleccionado = slotSeleccionado === slotUtc;
                    return (
                      <button
                        key={slotUtc}
                        type="button"
                        onClick={() => setSlotSeleccionado(slotUtc)}
                        className={
                          seleccionado
                            ? 'py-3 px-4 bg-primary text-on-primary rounded-xl font-body-sm shadow-md transition-all'
                            : 'py-3 px-4 border border-outline-variant rounded-xl text-on-surface font-body-sm hover:border-primary-container hover:bg-primary-container/5 transition-all'
                        }
                      >
                        {horaUtcALocal(slotUtc)}
                      </button>
                    );
                  })}
                </div>
              )}

              <p className="mt-6 text-center text-on-surface-variant font-label-md text-label-md flex items-center justify-center gap-2">
                <span className="material-symbols-outlined text-[18px]">info</span>
                Los horarios se muestran en tu hora local.
              </p>
            </section>
          </div>

          {/* Resumen */}
          <aside>
            <div className="bg-surface-container-high p-6 rounded-3xl soft-elevation border border-outline-variant/10 lg:sticky lg:top-8">
              <div className="mb-6 flex items-center gap-3">
                <div className="w-14 h-14 rounded-2xl bg-primary text-on-primary flex items-center justify-center font-title-md font-semibold shrink-0">
                  {tenant.nombre.charAt(0).toUpperCase()}
                </div>
                <h4 className="font-title-md text-on-surface">{tenant.nombre}</h4>
              </div>

              <div className="space-y-4 py-6 border-y border-outline-variant/20">
                <div className="flex justify-between items-start gap-4">
                  <span className="text-on-surface-variant font-body-sm shrink-0">Servicio</span>
                  <span className="text-on-surface font-semibold text-right">{nombresServiciosElegidos || '—'}</span>
                </div>
                <div className="flex justify-between items-center">
                  <span className="text-on-surface-variant font-body-sm">Fecha</span>
                  <span className="text-on-surface font-semibold capitalize">{fechaFormateada ?? '—'}</span>
                </div>
                <div className="flex justify-between items-center">
                  <span className="text-on-surface-variant font-body-sm">Hora</span>
                  <span className="text-on-surface font-semibold">
                    {slotSeleccionado ? horaUtcALocal(slotSeleccionado) : '—'}
                  </span>
                </div>
              </div>

              <div className="py-6 flex justify-between items-center text-primary font-bold">
                <span className="text-title-md">Total</span>
                <span className="text-title-md">${precioTotal}</span>
              </div>

              <button
                type="button"
                disabled={!puedeConfirmar || confirmando}
                onClick={confirmar}
                className="w-full py-4 bg-primary-container text-on-primary-container hover:opacity-90 active:scale-95 transition-all font-title-md rounded-2xl shadow-lg flex items-center justify-center gap-2 disabled:opacity-40 disabled:cursor-not-allowed disabled:active:scale-100"
              >
                {confirmando ? 'Confirmando...' : 'Confirmar Reserva'}
                <span className="material-symbols-outlined">arrow_forward</span>
              </button>

              <p className="mt-4 text-center text-outline font-label-md text-label-md">
                Vas a recibir un email de confirmación.
              </p>
            </div>
          </aside>
        </div>
      </div>
    </div>
  );
}
