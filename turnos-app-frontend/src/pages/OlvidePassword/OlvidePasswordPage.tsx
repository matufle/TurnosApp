// src/pages/OlvidePassword/OlvidePasswordPage.tsx
import { useState } from 'react';
import { Link } from 'react-router-dom';
import { Anchor, Button, Center, Stack, Text, TextInput, Title } from '@mantine/core';
import { useForm, isEmail } from '@mantine/form';
import { authService } from '../../api/authService';

export function OlvidePasswordPage() {
  const [loading, setLoading] = useState(false);
  const [enviado, setEnviado] = useState(false);

  const form = useForm({
    initialValues: { email: '' },
    validate: {
      email: isEmail('Ingresá un email válido'),
    },
  });

  const handleSubmit = async (values: typeof form.values) => {
    setLoading(true);
    try {
      await authService.olvidePassword(values.email);
    } finally {
      setLoading(false);
      // Anti-enumeración: mostramos éxito exista o no la cuenta, igual que hace el backend.
      setEnviado(true);
    }
  };

  return (
    <Center h="100vh" px="md">
      <Stack maw={400} w="100%" gap="md">
        <Title order={3}>¿Olvidaste tu contraseña?</Title>

        {enviado ? (
          <>
            <Text c="dimmed">
              Si el email que ingresaste corresponde a una cuenta, te enviamos un link para restablecer tu
              contraseña. Revisá tu casilla de entrada.
            </Text>
            <Anchor component={Link} to="/login">
              Volver a iniciar sesión
            </Anchor>
          </>
        ) : (
          <>
            <Text c="dimmed">Ingresá tu email y te enviamos un link para restablecer tu contraseña.</Text>
            <form onSubmit={form.onSubmit(handleSubmit)} noValidate>
              <Stack gap="sm">
                <TextInput label="Email" placeholder="tu@negocio.com" {...form.getInputProps('email')} />
                <Button type="submit" loading={loading} fullWidth>
                  Enviar link de recuperación
                </Button>
              </Stack>
            </form>
            <Anchor component={Link} to="/login" ta="center">
              Volver a iniciar sesión
            </Anchor>
          </>
        )}
      </Stack>
    </Center>
  );
}
