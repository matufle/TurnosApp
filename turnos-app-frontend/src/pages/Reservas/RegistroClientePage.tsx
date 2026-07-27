// src/pages/Reservas/RegistroClientePage.tsx
import { useState } from 'react';
import { Link, useNavigate, useOutletContext, useParams, useSearchParams } from 'react-router-dom';
import { Anchor, Alert, Button, Center, PasswordInput, Stack, TextInput, Title } from '@mantine/core';
import { useForm, isEmail, isNotEmpty, hasLength } from '@mantine/form';
import axios from 'axios';
import { clienteAuthService } from '../../api/clienteAuthService';
import { useClienteAuth } from '../../context/useClienteAuth';
import type { TenantPublico } from '../../types/ClienteAuth';

export function RegistroClientePage() {
  const { slug } = useParams<{ slug: string }>();
  const { tenant } = useOutletContext<{ tenant: TenantPublico }>();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const { login } = useClienteAuth();
  const [loading, setLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const form = useForm({
    initialValues: { nombre: '', apellido: '', email: '', telefono: '', password: '' },
    validate: {
      nombre: isNotEmpty('Ingresá tu nombre'),
      apellido: isNotEmpty('Ingresá tu apellido'),
      email: isEmail('Ingresá un email válido'),
      password: hasLength({ min: 8 }, 'La contraseña debe tener al menos 8 caracteres'),
    },
  });

  const handleSubmit = async (values: typeof form.values) => {
    setErrorMessage(null);
    setLoading(true);

    try {
      const response = await clienteAuthService.registrar({
        tenantSlug: tenant.slug,
        ...values,
        telefono: values.telefono || undefined,
      });
      await login(response.token, response.tenantId, tenant.slug);
      const returnTo = searchParams.get('returnTo');
      navigate(returnTo ? decodeURIComponent(returnTo) : `/reservas/${slug}/mis-turnos`, { replace: true });
    } catch (error) {
      if (axios.isAxiosError(error) && error.response?.status === 409) {
        setErrorMessage('Ya existe una cuenta registrada con ese email.');
      } else {
        setErrorMessage('No pudimos conectar con el servidor. Intentá de nuevo.');
      }
    } finally {
      setLoading(false);
    }
  };

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

            <Button type="submit" loading={loading} fullWidth>
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
