// src/pages/ConfirmarEmail/ConfirmarEmailPage.tsx
import { useEffect, useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { Anchor, Button, Center, Loader, Stack, Text, Title } from '@mantine/core';
import { authService } from '../../api/authService';

type Estado = 'confirmando' | 'exito' | 'error';

export function ConfirmarEmailPage() {
  const [searchParams] = useSearchParams();
  const token = searchParams.get('token');
  const [estado, setEstado] = useState<Estado>('confirmando');

  useEffect(() => {
    if (!token) {
      setEstado('error');
      return;
    }

    let cancelado = false;

    authService
      .confirmarEmail(token)
      .then(() => {
        if (!cancelado) setEstado('exito');
      })
      .catch(() => {
        if (!cancelado) setEstado('error');
      });

    return () => {
      cancelado = true;
    };
  }, [token]);

  return (
    <Center h="100vh" px="md">
      <Stack align="center" gap="md" maw={400} w="100%">
        {estado === 'confirmando' && (
          <>
            <Loader type="dots" />
            <Text c="dimmed">Confirmando tu email...</Text>
          </>
        )}

        {estado === 'exito' && (
          <>
            <Title order={3} ta="center">
              ¡Tu email fue confirmado!
            </Title>
            <Text c="dimmed" ta="center">
              Ya podés iniciar sesión con tu cuenta.
            </Text>
            <Button component={Link} to="/login">
              Ir a iniciar sesión
            </Button>
          </>
        )}

        {estado === 'error' && (
          <>
            <Title order={3} ta="center">
              No pudimos confirmar tu email
            </Title>
            <Text c="dimmed" ta="center">
              El link puede haber expirado o ser inválido. Probá iniciar sesión para reenviar la confirmación.
            </Text>
            <Anchor component={Link} to="/login">
              Ir a iniciar sesión
            </Anchor>
          </>
        )}
      </Stack>
    </Center>
  );
}
