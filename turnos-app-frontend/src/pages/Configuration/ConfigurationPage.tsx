// src/pages/Configuration/ConfigurationPage.tsx
import { useEffect, useState } from 'react';
import axios from 'axios';
import {
  Title,
  Button,
  Stack,
  Alert,
  Loader,
  Center,
  Paper,
  Text,
  Switch,
  ColorInput,
  Divider,
  TextInput,
  CopyButton,
  ActionIcon,
  Tooltip,
  Select,
} from '@mantine/core';
import { useForm } from '@mantine/form';
import { IconAlertCircle, IconDeviceFloppy, IconCopy, IconCheck } from '@tabler/icons-react';
import { tenantService } from '../../api/tenantService';
import { useTenantTheme } from '../../context/useTenantTheme';

export function ConfigurationPage() {
  const { setColorHex } = useTenantTheme();

  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [slug, setSlug] = useState<string | null>(null);

  const form = useForm({
    initialValues: {
      colorPrimario: '#0EA5E9',
      permiteReservasPublicas: false,
      permiteSolapamiento: false, // Unificado: usamos 'permite' en todo
      frecuenciaLiquidacion: 'Mensual' as 'Semanal' | 'Quincenal' | 'Mensual',
    },
  });

  useEffect(() => {
    let activo = true;

    const cargarConfiguracion = async () => {
      try {
        const data = await tenantService.getConfig();
        if (activo) {
          // Asegúrate de usar el nombre exacto que devuelve el backend
          form.setValues({
            colorPrimario: data.colorPrimario || '#0EA5E9',
            permiteReservasPublicas: data.permiteReservasPublicas || false,
            // Aquí usamos 'permiteSolapamiento' para que coincida con la propiedad
            permiteSolapamiento: data.permiteSolapamiento || false,
            frecuenciaLiquidacion: data.frecuenciaLiquidacion || 'Mensual',
          });
          setSlug(data.slug ?? null);
          setErrorMessage(null);
        }
      } catch {
        if (activo) {
          setErrorMessage('No pudimos cargar la configuración actual.');
        }
      } finally {
        if (activo) {
          setLoading(false);
        }
      }
    };

    cargarConfiguracion();

    return () => {
      activo = false;
    };
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const handleSubmit = async (values: typeof form.values) => {
    setSubmitting(true);
    setErrorMessage(null);
    setSuccessMessage(null);

    try {
      // Enviamos el objeto con el nombre unificado
      await tenantService.updateConfig(values);
      setSuccessMessage('¡Configuración guardada con éxito!');
      setColorHex(values.colorPrimario);
    } catch (error) {
      if (axios.isAxiosError(error) && error.response?.data?.detail) {
        setErrorMessage(error.response.data.detail);
      } else {
        setErrorMessage('Hubo un problema al guardar los cambios.');
      }
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Stack gap="lg" maw={600}>
      <Title order={2}>Configuración del Sistema</Title>

      {errorMessage && (
        <Alert icon={<IconAlertCircle size={16} />} color="red" variant="light">
          {errorMessage}
        </Alert>
      )}

      {successMessage && (
        <Alert color="green" variant="light">
          {successMessage}
        </Alert>
      )}

      {loading ? (
        <Center py="xl">
          <Loader />
        </Center>
      ) : (
        <Paper withBorder radius="md" p="xl">
          <form onSubmit={form.onSubmit(handleSubmit)}>
            <Stack gap="xl">
              <div>
                <Title order={4} mb="xs">Identidad Visual</Title>
                <Text size="sm" c="dimmed" mb="md">
                  Personalizá el color principal que verán tus clientes.
                </Text>
                <ColorInput
                  data-tour="config-color"
                  label="Color Primario"
                  format="hex"
                  swatches={['#0EA5E9', '#12b886', '#fab005', '#fd7e14', '#fa5252', '#be4bdb', '#7950f2']}
                  {...form.getInputProps('colorPrimario')}
                />
              </div>

              <Divider />

              <div>
                <Title order={4} mb="xs">Reglas de Negocio</Title>
                <Text size="sm" c="dimmed" mb="md">
                  Ajustá cómo funciona la agenda y quién puede interactuar con ella.
                </Text>
                <Stack gap="md">
                  <Switch
                    label="Permitir Reservas Públicas"
                    description="Si está activo, los clientes podrán autogestionar sus turnos."
                    size="md"
                    {...form.getInputProps('permiteReservasPublicas', { type: 'checkbox' })}
                  />

                  {slug && (
                    <TextInput
                      label="Link de reservas"
                      description={
                        form.values.permiteReservasPublicas
                          ? 'Compartí este link con tus clientes para que reserven online.'
                          : 'Activá "Permitir Reservas Públicas" para que este link funcione.'
                      }
                      readOnly
                      value={`${window.location.origin}/reservas/${slug}`}
                      rightSection={
                        <CopyButton value={`${window.location.origin}/reservas/${slug}`} timeout={1500}>
                          {({ copied, copy }) => (
                            <Tooltip label={copied ? 'Copiado' : 'Copiar'} withArrow>
                              <ActionIcon color={copied ? 'teal' : 'gray'} variant="subtle" onClick={copy}>
                                {copied ? <IconCheck size={16} /> : <IconCopy size={16} />}
                              </ActionIcon>
                            </Tooltip>
                          )}
                        </CopyButton>
                      }
                    />
                  )}

                  <Switch
                    data-tour="config-solapamiento"
                    label="Permitir turnos en simultáneo (Solapamiento)"
                    description="Si está activo, permitirá múltiples turnos en el mismo horario."
                    size="md"
                    {...form.getInputProps('permiteSolapamiento', { type: 'checkbox' })}
                  />
                </Stack>
              </div>

              <Divider />

              <div>
                <Title order={4} mb="xs">Liquidaciones</Title>
                <Text size="sm" c="dimmed" mb="md">
                  Cada cuánto se generan automáticamente las liquidaciones de comisión de los profesionales.
                </Text>
                <Select
                  label="Frecuencia de liquidación"
                  data={[
                    { value: 'Semanal', label: 'Semanal (lunes a domingo)' },
                    { value: 'Quincenal', label: 'Quincenal (1-15 / 16-fin de mes)' },
                    { value: 'Mensual', label: 'Mensual' },
                  ]}
                  allowDeselect={false}
                  {...form.getInputProps('frecuenciaLiquidacion')}
                />
              </div>

              <Button
                type="submit"
                loading={submitting}
                leftSection={<IconDeviceFloppy size={18} />}
                mt="md"
              >
                Guardar Cambios
              </Button>
            </Stack>
          </form>
        </Paper>
      )}
    </Stack>
  );
}