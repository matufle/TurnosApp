// src/pages/Reservas/RegistroClientePage.tsx
import { useState } from 'react';
import { Link, useOutletContext, useParams, useSearchParams } from 'react-router-dom';
import { Anchor, Alert, Button, Center, Checkbox, PasswordInput, Stack, Text, TextInput, Title } from '@mantine/core';
import { useForm, isEmail, isNotEmpty, hasLength } from '@mantine/form';
import axios from 'axios';
import { clienteAuthService } from '../../api/clienteAuthService';
import { TurnstileWidget } from '../../components/TurnstileWidget';
import type { TenantPublico } from '../../types/ClienteAuth';

export function RegistroClientePage() {
  const { slug } = useParams<{ slug: string }>();
  const { tenant } = useOutletContext<{ tenant: TenantPublico }>();
  const [searchParams] = useSearchParams();
  const [loading, setLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [registroPendienteEmail, setRegistroPendienteEmail] = useState<string | null>(null);
  const [reenviando, setReenviando] = useState(false);
  const [reenviado, setReenviado] = useState(false);
  const [turnstileToken, setTurnstileToken] = useState<string | null>(null);

  const form = useForm({
    initialValues: { nombre: '', apellido: '', email: '', telefono: '', password: '', aceptaTerminos: false },
    validate: {
      nombre: isNotEmpty('Ingresá tu nombre'),
      apellido: isNotEmpty('Ingresá tu apellido'),
      email: isEmail('Ingresá un email válido'),
      password: hasLength({ min: 8 }, 'La contraseña debe tener al menos 8 caracteres'),
      aceptaTerminos: (value) => (value ? null : 'Tenés que aceptar los términos para continuar'),
    },
  });

  const handleSubmit = async (values: typeof form.values) => {
    if (!turnstileToken) {
      setErrorMessage('Completá la verificación de seguridad para continuar.');
      return;
    }

    setErrorMessage(null);
    setLoading(true);

    try {
      const { aceptaTerminos: _aceptaTerminos, ...datosCliente } = values;
      const response = await clienteAuthService.registrar({
        tenantSlug: tenant.slug,
        ...datosCliente,
        telefono: values.telefono || undefined,
        turnstileToken,
      });
      setRegistroPendienteEmail(response.email);
    } catch (error) {
      if (axios.isAxiosError(error)) {
        const code = error.response?.data?.code as string | undefined;
        if (code === 'CAPTCHA_INVALIDO') {
          setErrorMessage('No pudimos verificar que sos humano. Intentá de nuevo.');
        } else if (error.response?.status === 409) {
          setErrorMessage('Ya existe una cuenta registrada con ese email.');
        } else {
          setErrorMessage('No pudimos conectar con el servidor. Intentá de nuevo.');
        }
      } else {
        setErrorMessage('No pudimos conectar con el servidor. Intentá de nuevo.');
      }
    } finally {
      setLoading(false);
    }
  };

  const handleReenviar = async () => {
    if (!registroPendienteEmail) return;
    setReenviando(true);
    try {
      await clienteAuthService.reenviarConfirmacion(tenant.slug, registroPendienteEmail);
      setReenviado(true);
    } finally {
      setReenviando(false);
    }
  };

  if (registroPendienteEmail) {
    return (
      <Center py="xl" px="md">
        <Stack maw={360} w="100%" gap="md" align="center" ta="center">
          <Title order={3}>Revisá tu email</Title>
          <Text c="dimmed">
            Te enviamos un link de confirmación a <strong>{registroPendienteEmail}</strong>. Confirmá tu cuenta para
            poder reservar turnos.
          </Text>
          {reenviado ? (
            <Text c="dimmed" size="sm">
              Listo, si correspondía te reenviamos el email.
            </Text>
          ) : (
            <Anchor component="button" type="button" onClick={handleReenviar} disabled={reenviando}>
              {reenviando ? 'Reenviando...' : '¿No te llegó? Reenviar email'}
            </Anchor>
          )}
          <Anchor component={Link} to={`/reservas/${slug}/login`}>
            Ir a iniciar sesión
          </Anchor>
        </Stack>
      </Center>
    );
  }

  return (
    <Center py="xl" px="md">
      <Stack maw={360} w="100%" gap="md">
        <Title order={3}>Creá tu cuenta</Title>

        <form onSubmit={form.onSubmit(handleSubmit)} noValidate>
          <Stack gap="sm">
            {errorMessage && <Alert color="red">{errorMessage}</Alert>}

            <TextInput label="Nombre" {...form.getInputProps('nombre')} />
            <TextInput label="Apellido" {...form.getInputProps('apellido')} />
            <TextInput label="Email" placeholder="tu@email.com" {...form.getInputProps('email')} />
            <TextInput label="Teléfono (opcional)" {...form.getInputProps('telefono')} />
            <PasswordInput label="Contraseña" {...form.getInputProps('password')} />

            <Checkbox
              label={
                <>
                  Acepto los{' '}
                  <Anchor href="/terminos" target="_blank" rel="noopener noreferrer" onClick={(e) => e.stopPropagation()}>
                    Términos de Servicio
                  </Anchor>{' '}
                  y la{' '}
                  <Anchor href="/privacidad" target="_blank" rel="noopener noreferrer" onClick={(e) => e.stopPropagation()}>
                    Política de Privacidad
                  </Anchor>
                </>
              }
              {...form.getInputProps('aceptaTerminos', { type: 'checkbox' })}
            />

            <TurnstileWidget onVerify={setTurnstileToken} onExpire={() => setTurnstileToken(null)} />

            <Button type="submit" loading={loading} disabled={!turnstileToken} fullWidth>
              Registrarme
            </Button>
          </Stack>
        </form>

        <Anchor
          component={Link}
          to={`/reservas/${slug}/login${searchParams.get('returnTo') ? `?returnTo=${searchParams.get('returnTo')}` : ''}`}
          ta="center"
        >
          ¿Ya tenés cuenta? Ingresá
        </Anchor>
      </Stack>
    </Center>
  );
}
