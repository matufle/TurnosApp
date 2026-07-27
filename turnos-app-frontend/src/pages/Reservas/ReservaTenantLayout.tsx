// src/pages/Reservas/ReservaTenantLayout.tsx
// Punto de entrada mínimo de la Parte 2: resuelve el tenant por slug y expone el
// resultado a las rutas hijas (login/registro/mis-turnos) vía Outlet context.
// Sin inversión de diseño — la Parte 3 rehace esto visualmente con branding real.
import { useEffect, useState } from 'react';
import { Outlet, useParams } from 'react-router-dom';
import { Center, Loader, Stack, Text, Title } from '@mantine/core';
import { publicTenantService } from '../../api/publicTenantService';
import { ClienteAuthProvider } from '../../context/ClienteAuthContext';
import type { TenantPublico } from '../../types/ClienteAuth';

export function ReservaTenantLayout() {
  const { slug } = useParams<{ slug: string }>();
  const [tenant, setTenant] = useState<TenantPublico | null>(null);
  const [loading, setLoading] = useState(true);
  const [notFound, setNotFound] = useState(false);

  useEffect(() => {
    if (!slug) return;

    let cancelado = false;
    setLoading(true);
    setNotFound(false);

    publicTenantService
      .getBySlug(slug)
      .then((data) => {
        if (!cancelado) setTenant(data);
      })
      .catch(() => {
        if (!cancelado) setNotFound(true);
      })
      .finally(() => {
        if (!cancelado) setLoading(false);
      });

    return () => {
      cancelado = true;
    };
  }, [slug]);

  if (loading) {
    return (
      <Center h="100vh">
        <Loader type="dots" />
      </Center>
    );
  }

  if (notFound || !tenant) {
    return (
      <Center h="100vh">
        <Stack align="center" gap="xs">
          <Title order={3}>Página no encontrada</Title>
          <Text c="dimmed">Este negocio no tiene reservas online habilitadas.</Text>
        </Stack>
      </Center>
    );
  }

  return (
    <ClienteAuthProvider>
      <Stack gap={0} mih="100vh">
        <Center py="md" style={{ borderBottom: '1px solid var(--mantine-color-gray-3)' }}>
          <Title order={4}>{tenant.nombre}</Title>
        </Center>
        <Outlet context={{ tenant } satisfies { tenant: TenantPublico }} />
      </Stack>
    </ClienteAuthProvider>
  );
}
