// src/pages/Reservas/ReservarPage.tsx
// Detrás de ClienteProtectedRoute: reserva obligatoria, como pide el enunciado original.
// El servicio ?servicioId= de la URL (elegido en CatalogoPage) es solo el punto de partida:
// el cliente puede sumar más servicios al mismo turno acá, en paridad con el flujo de staff
// en TurnosPage.tsx (que ya soportaba multi-servicio desde el principio).
import { useEffect, useMemo, useState } from 'react';
import { useNavigate, useParams, useSearchParams } from 'react-router-dom';
import { Alert, Button, Center, Group, Loader, MultiSelect, Select, SimpleGrid, Stack, Text, Title } from '@mantine/core';
import { DatePickerInput } from '@mantine/dates';
import axios from 'axios';
import { publicCatalogoService } from '../../api/publicCatalogoService';
import { misTurnosService } from '../../api/misTurnosService';
import { trackEvent } from '../../lib/analytics';
import { horaUtcALocal } from '../../utils/horarioTimezone';
import type { Servicio } from '../../types/Servicio';
import type { RecursoPublico } from '../../types/ReservaPublica';

export function ReservarPage() {
  const { slug } = useParams<{ slug: string }>();
  const [searchParams] = useSearchParams();
  const servicioIdInicial = Number(searchParams.get('servicioId'));
  const navigate = useNavigate();

  const [servicios, setServicios] = useState<Servicio[]>([]);
  const [servicioIdsSeleccionados, setServicioIdsSeleccionados] = useState<string[]>([]);
  const [recursos, setRecursos] = useState<RecursoPublico[]>([]);
  const [recursoId, setRecursoId] = useState<string | null>(null);
  const [fecha, setFecha] = useState<string | null>(null);
  const [slotsUtc, setSlotsUtc] = useState<string[]>([]);
  const [cargandoSlots, setCargandoSlots] = useState(false);
  const [cargandoInicial, setCargandoInicial] = useState(true);
  const [slotConfirmando, setSlotConfirmando] = useState<string | null>(null);
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

  const confirmar = async (slotUtc: string) => {
    if (!slug || !recursoId || !fecha || servicioIds.length === 0) return;

    setErrorMessage(null);
    setSlotConfirmando(slotUtc);

    try {
      // slotUtc ya viene en la misma convención "UTC-equivalente" que usa el backend —
      // combinarlo directo con la fecha arma el instante UTC correcto (ver horarioTimezone.ts).
      await misTurnosService.crear({
        recursoId: Number(recursoId),
        servicioIds,
        fechaHoraInicio: `${fecha}T${slotUtc}:00.000Z`,
      });
      trackEvent('Appointment Created', { origen: 'self-service' });
      navigate(`/reservas/${slug}/mis-turnos`, { replace: true });
    } catch (error) {
      if (axios.isAxiosError(error) && error.response?.status === 409) {
        setErrorMessage('Ese horario ya no está disponible. Elegí otro.');
        setSlotsUtc((prev) => prev.filter((s) => s !== slotUtc));
      } else {
        setErrorMessage('No pudimos confirmar la reserva. Intentá de nuevo.');
      }
    } finally {
      setSlotConfirmando(null);
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

  return (
    <Stack maw={420} w="100%" mx="auto" py="xl" px="md" gap="md">
      <Title order={3}>Reservar turno</Title>

      <MultiSelect
        label="Servicios"
        placeholder="Elegí uno o más servicios"
        data={servicios.map((s) => ({ value: String(s.id), label: `${s.nombre} (${s.duracionMinutos} min · $${s.precio})` }))}
        value={servicioIdsSeleccionados}
        onChange={setServicioIdsSeleccionados}
      />

      {servicioIds.length > 0 && (
        <Text size="sm" c="dimmed">
          Duración total: {duracionTotal} min · Precio total: ${precioTotal}
        </Text>
      )}

      {recursos.length > 1 && (
        <Select
          label="Con quién"
          data={recursos.map((r) => ({ value: String(r.id), label: r.nombre }))}
          value={recursoId}
          onChange={setRecursoId}
        />
      )}

      <DatePickerInput label="Fecha" value={fecha} onChange={setFecha} minDate={new Date()} />

      {errorMessage && <Alert color="red">{errorMessage}</Alert>}

      {servicioIds.length === 0 && <Text c="dimmed">Elegí al menos un servicio para ver los horarios disponibles.</Text>}

      {cargandoSlots && (
        <Center py="md">
          <Loader type="dots" />
        </Center>
      )}

      {!cargandoSlots && recursoId && fecha && servicioIds.length > 0 && slotsUtc.length === 0 && (
        <Text c="dimmed">No hay horarios disponibles ese día.</Text>
      )}

      {!cargandoSlots && slotsUtc.length > 0 && (
        <SimpleGrid cols={{ base: 3, xs: 4 }} spacing="xs">
          {slotsUtc.map((slotUtc) => (
            <Button
              key={slotUtc}
              variant="light"
              size="sm"
              loading={slotConfirmando === slotUtc}
              disabled={slotConfirmando !== null && slotConfirmando !== slotUtc}
              onClick={() => confirmar(slotUtc)}
            >
              {horaUtcALocal(slotUtc)}
            </Button>
          ))}
        </SimpleGrid>
      )}

      <Group justify="center">
        <Text size="xs" c="dimmed">
          Los horarios se muestran en tu hora local.
        </Text>
      </Group>
    </Stack>
  );
}
