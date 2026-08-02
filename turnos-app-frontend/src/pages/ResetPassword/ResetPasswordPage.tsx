// src/pages/ResetPassword/ResetPasswordPage.tsx
import { useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { Alert, Anchor, Button, Center, PasswordInput, Stack, Text, Title } from '@mantine/core';
import { useForm, hasLength, matchesField } from '@mantine/form';
import axios from 'axios';
import { authService } from '../../api/authService';

type Estado = 'formulario' | 'exito' | 'token-invalido';

export function ResetPasswordPage() {
  const [searchParams] = useSearchParams();
  const token = searchParams.get('token');
  const [estado, setEstado] = useState<Estado>(token ? 'formulario' : 'token-invalido');
  const [loading, setLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const form = useForm({
    initialValues: { password: '', confirmarPassword: '' },
    validate: {
      password: hasLength({ min: 8 }, 'La contraseña debe tener al menos 8 caracteres'),
      confirmarPassword: matchesField('password', 'Las contraseñas no coinciden'),
    },
  });

  const handleSubmit = async (values: typeof form.values) => {
    if (!token) return;
    setErrorMessage(null);
    setLoading(true);

    try {
      await authService.resetPassword(token, values.password);
      setEstado('exito');
    } catch (error) {
      if (axios.isAxiosError(error) && error.response?.status === 409) {
        setEstado('token-invalido');
      } else {
        setErrorMessage('No pudimos conectar con el servidor. Intentá de nuevo.');
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <Center h="100vh" px="md">
      <Stack maw={400} w="100%" gap="md">
        {estado === 'formulario' && (
          <>
            <Title order={3}>Restablecé tu contraseña</Title>
            <form onSubmit={form.onSubmit(handleSubmit)} noValidate>
              <Stack gap="sm">
                {errorMessage && <Alert color="red">{errorMessage}</Alert>}
                <PasswordInput label="Nueva contraseña" {...form.getInputProps('password')} />
                <PasswordInput label="Confirmá la nueva contraseña" {...form.getInputProps('confirmarPassword')} />
                <Button type="submit" loading={loading} fullWidth>
                  Restablecer contraseña
                </Button>
              </Stack>
            </form>
          </>
        )}

        {estado === 'exito' && (
          <>
            <Title order={3} ta="center">
              ¡Contraseña restablecida!
            </Title>
            <Text c="dimmed" ta="center">
              Ya podés iniciar sesión con tu nueva contraseña.
            </Text>
            <Button component={Link} to="/login">
              Ir a iniciar sesión
            </Button>
          </>
        )}

        {estado === 'token-invalido' && (
          <>
            <Title order={3} ta="center">
              Este link no es válido
            </Title>
            <Text c="dimmed" ta="center">
              Puede haber expirado o ya haberse usado. Pedí uno nuevo para continuar.
            </Text>
            <Anchor component={Link} to="/olvide-password">
              Pedir un nuevo link
            </Anchor>
          </>
        )}
      </Stack>
    </Center>
  );
}
