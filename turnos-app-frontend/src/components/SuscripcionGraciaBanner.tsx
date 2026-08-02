// src/components/SuscripcionGraciaBanner.tsx
import { useEffect, useState } from 'react';
import { Alert, Button, Group } from '@mantine/core';
import { IconAlertTriangle } from '@tabler/icons-react';
import { useNavigate, useLocation } from 'react-router-dom';
import { suscripcionService } from '../api/suscripcionService';
import { usePermission } from '../hooks/usePermission';

/// Aviso persistente en /app/* cuando la suscripción está en PastDue pero todavía dentro del
/// período de gracia (ver SuscripcionConstantes.DiasGraciaPastDue en el backend) — una vez que
/// la gracia se vence, el filtro RequiereSuscripcionActivaAttribute empieza a devolver 409 en
/// todo, y httpClient.ts redirige directo a /app/suscripcion.
export function SuscripcionGraciaBanner() {
  const navigate = useNavigate();
  const location = useLocation();
  const puedeGestionar = usePermission('GestionarSuscripcion');
  const [diasRestantes, setDiasRestantes] = useState<number | null>(null);

  useEffect(() => {
    let activo = true;

    suscripcionService.getEstado()
      .then((data) => {
        if (!activo) return;
        if (data.estadoSuscripcion === 'PastDue' && data.diasRestantesGracia !== null) {
          setDiasRestantes(data.diasRestantesGracia);
        }
      })
      .catch(() => {
        // Silencioso: el banner es un aviso extra, no algo crítico para bloquear la carga de la página.
      });

    return () => { activo = false; };
  }, [location.pathname]);

  if (diasRestantes === null || location.pathname.startsWith('/app/suscripcion')) {
    return null;
  }

  const mensaje = diasRestantes > 0
    ? `No pudimos procesar el pago de tu suscripción. Tenés ${diasRestantes} ${diasRestantes === 1 ? 'día' : 'días'} para actualizar tu método de pago antes de perder el acceso.`
    : 'No pudimos procesar el pago de tu suscripción y el período de gracia terminó. Actualizá tu método de pago para no perder el acceso.';

  return (
    <Alert
      icon={<IconAlertTriangle size={16} />}
      color="yellow"
      variant="light"
      mb="md"
    >
      <Group justify="space-between" wrap="wrap" gap="sm">
        <span>{mensaje}</span>
        {puedeGestionar && (
          <Button size="xs" color="yellow" variant="filled" onClick={() => navigate('/app/suscripcion')}>
            Actualizar método de pago
          </Button>
        )}
      </Group>
    </Alert>
  );
}
