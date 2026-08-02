// src/pages/Reservas/LoginClientePage.tsx
import { useState } from 'react';
import { Link, useNavigate, useOutletContext, useParams, useSearchParams } from 'react-router-dom';
import { Anchor, Alert, Button, Center, Checkbox, PasswordInput, Stack, Text, TextInput, Title } from '@mantine/core';
import { useForm, isEmail, isNotEmpty } from '@mantine/form';
import axios from 'axios';
import { clienteAuthService } from '../../api/clienteAuthService';
import { useClienteAuth } from '../../context/useClienteAuth';
import type { TenantPublico } from '../../types/ClienteAuth';

export function LoginClientePage() {
  const { slug } = useParams<{ slug: string }>();
  const { tenant } = useOutletContext<{ tenant: TenantPublico }>();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const { login } = useClienteAuth();
  const [loading, setLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [emailNoConfirmado, setEmailNoConfirmado] = useState<string | null>(null);
  const [reenviando, setReenviando] = useState(false);
  const [reenviado, setReenviado] = useState(false);

  const form = useForm({
    initialValues: { email: '', password: '', recordarMe: false },
    validate: {
      email: isEmail('Ingresá un email válido'),
      password: isNotEmpty('La contraseña es obligatoria'),
    },
  });

  const handleSubmit = async (values: typeof form.values) => {
    setErrorMessage(null);
    setEmailNoConfirmado(null);
    setReenviado(false);
    setLoading(true);

    try {
      const response = await clienteAuthService.login({ tenantSlug: tenant.slug, ...values });
      await login(response.token, response.tenantId, tenant.slug);
      const returnTo = searchParams.get('returnTo');
      navigate(returnTo ? decodeURIComponent(returnTo) : `/reservas/${slug}/mis-turnos`, { replace: true });
    } catch (error) {
      if (axios.isAxiosError(error) && error.response?.status === 409) {
        const code = error.response.data?.code as string | undefined;
        if (code === 'EMAIL_NO_CONFIRMADO') {
          setEmailNoConfirmado(values.email);
        } else {
          setErrorMessage('Email o contraseña incorrectos.');
        }
      } else {
        setErrorMessage('No pudimos conectar con el servidor. Intentá de nuevo.');
      }
    } finally {
      setLoading(false);
    }
  };

  const handleReenviar = async () => {
    if (!emailNoConfirmado) return;
    setReenviando(true);
    try {
      await clienteAuthService.reenviarConfirmacion(tenant.slug, emailNoConfirmado);
      setReenviado(true);
    } finally {
      setReenviando(false);
    }
  };

  return (
    <Center py="xl" px="md">
      <Stack maw={360} w="100%" gap="md">
        <Title order={3}>Ingresá a tu cuenta</Title>

        <form onSubmit={form.onSubmit(handleSubmit)} noValidate>
          <Stack gap="sm">
            {errorMessage && <Alert color="red">{errorMessage}</Alert>}

            {emailNoConfirmado && (
              <Alert color="orange">
                <Stack gap="xs">
                  <Text size="sm">Confirmá tu email antes de ingresar. Revisá tu casilla de entrada.</Text>
                  {reenviado ? (
                    <Text size="sm" c="dimmed">
                      Listo, si correspondía te reenviamos el email.
                    </Text>
                  ) : (
                    <Anchor component="button" type="button" size="sm" onClick={handleReenviar} disabled={reenviando}>
                      {reenviando ? 'Reenviando...' : 'Reenviar email de confirmación'}
                    </Anchor>
                  )}
                </Stack>
              </Alert>
            )}

            <TextInput label="Email" placeholder="tu@email.com" {...form.getInputProps('email')} />
            <PasswordInput label="Contraseña" {...form.getInputProps('password')} />
            <Anchor component={Link} to={`/reservas/${slug}/olvide-password`} size="sm">
              ¿Olvidaste tu contraseña?
            </Anchor>
            <Checkbox
              label="Recordarme"
              {...form.getInputProps('recordarMe', { type: 'checkbox' })}
            />

            <Button type="submit" loading={loading} fullWidth>
              Ingresar
            </Button>
          </Stack>
        </form>

        <Anchor
          component={Link}
          to={`/reservas/${slug}/registro${searchParams.get('returnTo') ? `?returnTo=${searchParams.get('returnTo')}` : ''}`}
          ta="center"
        >
          ¿No tenés cuenta? Registrate
        </Anchor>
      </Stack>
    </Center>
  );
}
