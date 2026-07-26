// src/pages/MetodosPago/MetodosPagoPage.tsx
import { useCallback, useEffect, useState } from 'react';
import axios from 'axios';
import { Modal, TextInput, Select, NumberInput, Button, Menu, ActionIcon } from '@mantine/core';
import { useDisclosure } from '@mantine/hooks';
import { useForm, isNotEmpty } from '@mantine/form';
import { IconDots, IconEdit, IconBan, IconCreditCard } from '@tabler/icons-react';
import { metodosPagoService } from '../../api/metodosPagoService';
import { EmptyState } from '../../components/EmptyState';
import type { MetodoPago, TipoModificadorPago } from '../../types/MetodoPago';

const TIPO_MODIFICADOR_DATA: { value: TipoModificadorPago; label: string }[] = [
  { value: 'Ninguno', label: 'Ninguno' },
  { value: 'Bonificacion', label: 'Bonificación (descuento al cliente)' },
  { value: 'Recargo', label: 'Recargo (costo extra al cliente)' },
];

function iconoPorNombre(nombre: string) {
  const n = nombre.toLowerCase();
  if (n.includes('efectivo')) return 'payments';
  if (n.includes('tarjeta') || n.includes('credito') || n.includes('crédito') || n.includes('debito') || n.includes('débito'))
    return 'credit_card';
  if (n.includes('transferencia') || n.includes('banco')) return 'account_balance';
  return 'payments';
}

interface MetodoPagoFormValues {
  nombre: string;
  tipoModificador: TipoModificadorPago;
  porcentajeModificador: number;
  porcentajeComision: number;
}

export function MetodosPagoPage() {
  const [metodos, setMetodos] = useState<MetodoPago[]>([]);
  const [loading, setLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [metodoEditando, setMetodoEditando] = useState<MetodoPago | null>(null);
  const [modalOpened, { open, close }] = useDisclosure(false);

  const form = useForm<MetodoPagoFormValues>({
    initialValues: { nombre: '', tipoModificador: 'Ninguno', porcentajeModificador: 0, porcentajeComision: 0 },
    validate: {
      nombre: isNotEmpty('El nombre es obligatorio'),
      porcentajeModificador: (value) => (value >= 0 ? null : 'No puede ser negativo'),
      porcentajeComision: (value) => (value >= 0 ? null : 'No puede ser negativo'),
    },
  });

  const cargarMetodos = useCallback(async () => {
    try {
      const data = await metodosPagoService.getAll();
      setMetodos(data);
      setErrorMessage(null);
    } catch {
      setErrorMessage('No pudimos cargar los métodos de pago.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    cargarMetodos();
  }, [cargarMetodos]);

  const abrirCrear = () => {
    form.reset();
    setMetodoEditando(null);
    open();
  };

  const abrirEditar = (metodo: MetodoPago) => {
    form.setValues({
      nombre: metodo.nombre,
      tipoModificador: metodo.tipoModificador,
      porcentajeModificador: metodo.porcentajeModificador,
      porcentajeComision: metodo.porcentajeComision,
    });
    setMetodoEditando(metodo);
    open();
  };

  const handleSubmit = async (values: MetodoPagoFormValues) => {
    setSubmitting(true);
    try {
      if (metodoEditando) {
        await metodosPagoService.update(metodoEditando.id, { ...values, activo: metodoEditando.activo });
      } else {
        await metodosPagoService.create(values);
      }
      close();
      form.reset();
      setMetodoEditando(null);
      await cargarMetodos();
    } catch (error) {
      if (axios.isAxiosError(error) && error.response?.data?.detail) {
        form.setFieldError('nombre', error.response.data.detail);
      } else {
        setErrorMessage('No pudimos guardar los cambios.');
      }
    } finally {
      setSubmitting(false);
    }
  };

  const handleDesactivar = async (metodo: MetodoPago) => {
    try {
      const actualizado = await metodosPagoService.desactivar(metodo.id);
      setMetodos((prev) => prev.map((m) => (m.id === actualizado.id ? actualizado : m)));
    } catch {
      setErrorMessage('No pudimos desactivar el método de pago.');
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center py-24">
        <span className="material-symbols-outlined animate-spin text-primary text-4xl">progress_activity</span>
      </div>
    );
  }

  return (
    <div className="flex flex-col gap-10 pb-12">
      <header className="flex flex-col md:flex-row justify-between items-start md:items-end gap-6">
        <div>
          <h1 className="font-headline-lg text-headline-lg text-on-surface">Métodos de Pago</h1>
          <p className="font-body-lg text-body-lg text-secondary mt-2">Configura cómo cobras a tus clientes.</p>
        </div>
        <button
          onClick={abrirCrear}
          className="bg-primary text-on-primary font-label-md text-label-md px-6 py-3 rounded-full soft-elevation hover:bg-primary-container hover:shadow-lg transition-all duration-200 flex items-center gap-2 whitespace-nowrap w-full md:w-auto justify-center"
        >
          <span className="material-symbols-outlined">add</span> Añadir Método de Pago
        </button>
      </header>

      {errorMessage && (
        <div
          role="alert"
          className="flex items-center gap-2 rounded-lg border border-error/30 bg-error-container px-4 py-3 font-body-sm text-body-sm text-on-error-container"
        >
          <span className="material-symbols-outlined text-[20px]">error</span>
          {errorMessage}
        </div>
      )}

      {metodos.length === 0 ? (
        <EmptyState
          title="Sin métodos de pago"
          description="Creá tus métodos de pago (Efectivo, Tarjeta, MercadoPago...) para poder registrar cobros en tus turnos."
          icon={IconCreditCard}
          actionLabel="Añadir método de pago"
          onAction={abrirCrear}
        />
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-gutter">
          {metodos.map((metodo) => (
            <div
              key={metodo.id}
              className="bg-surface-container-lowest rounded-3xl p-6 soft-elevation flex flex-col gap-6 relative overflow-hidden"
            >
              <div className={`absolute left-0 top-0 bottom-0 w-2 ${metodo.activo ? 'bg-primary' : 'bg-outline-variant'}`} />

              <div className={`flex justify-between items-start ml-2 ${metodo.activo ? '' : 'opacity-60'}`}>
                <div className="flex items-center gap-3">
                  <div
                    className={`w-12 h-12 rounded-full flex items-center justify-center ${
                      metodo.activo ? 'bg-surface-container-low text-primary' : 'bg-surface text-outline'
                    }`}
                  >
                    <span className="material-symbols-outlined" style={{ fontVariationSettings: "'FILL' 1" }}>
                      {iconoPorNombre(metodo.nombre)}
                    </span>
                  </div>
                  <div>
                    <h3 className="font-title-md text-title-md text-on-surface">{metodo.nombre}</h3>
                    <span
                      className={`inline-flex items-center px-2 py-0.5 rounded-full font-label-md text-[10px] mt-1 ${
                        metodo.activo
                          ? 'bg-secondary-container text-on-secondary-container'
                          : 'bg-surface-variant text-on-surface-variant'
                      }`}
                    >
                      {metodo.activo ? 'Activo' : 'Inactivo'}
                    </span>
                  </div>
                </div>

                <Menu shadow="md" position="bottom-end" withinPortal>
                  <Menu.Target>
                    <ActionIcon variant="subtle" color="gray" aria-label="Más opciones">
                      <IconDots size={18} />
                    </ActionIcon>
                  </Menu.Target>
                  <Menu.Dropdown>
                    <Menu.Item leftSection={<IconEdit size={16} />} onClick={() => abrirEditar(metodo)}>
                      Editar
                    </Menu.Item>
                    {metodo.activo && (
                      <Menu.Item
                        leftSection={<IconBan size={16} />}
                        color="red"
                        onClick={() => handleDesactivar(metodo)}
                      >
                        Desactivar
                      </Menu.Item>
                    )}
                  </Menu.Dropdown>
                </Menu>
              </div>

              <div className={`grid grid-cols-2 gap-4 ml-2 ${metodo.activo ? '' : 'opacity-60'}`}>
                <div className="bg-surface p-3 rounded-xl border border-outline-variant/30">
                  <p className="font-label-md text-[10px] text-secondary uppercase tracking-wider mb-1">Modificador</p>
                  <p
                    className={`font-title-md text-body-lg flex items-center gap-1 ${
                      metodo.tipoModificador === 'Bonificacion'
                        ? 'text-tertiary-container'
                        : metodo.tipoModificador === 'Recargo'
                          ? 'text-error'
                          : 'text-secondary'
                    }`}
                  >
                    <span className="material-symbols-outlined text-[16px]">
                      {metodo.tipoModificador === 'Bonificacion'
                        ? 'arrow_downward'
                        : metodo.tipoModificador === 'Recargo'
                          ? 'arrow_upward'
                          : 'horizontal_rule'}
                    </span>
                    {metodo.tipoModificador === 'Bonificacion'
                      ? 'Bonificación'
                      : metodo.tipoModificador === 'Recargo'
                        ? 'Recargo'
                        : 'Ninguno'}
                  </p>
                  <p className="font-title-md text-title-md text-on-surface mt-1">
                    {metodo.tipoModificador === 'Bonificacion' ? '-' : metodo.tipoModificador === 'Recargo' ? '+' : ''}
                    {metodo.porcentajeModificador.toFixed(2)}%
                  </p>
                </div>
                <div className="bg-surface p-3 rounded-xl border border-outline-variant/30">
                  <p className="font-label-md text-[10px] text-secondary uppercase tracking-wider mb-1">
                    Comisión (Costo)
                  </p>
                  <p className="font-title-md text-body-lg text-secondary flex items-center gap-1">
                    <span className="material-symbols-outlined text-[16px]">account_balance</span>
                  </p>
                  <p className="font-title-md text-title-md text-on-surface mt-1">
                    {metodo.porcentajeComision.toFixed(2)}%
                  </p>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}

      <Modal
        opened={modalOpened}
        onClose={() => {
          close();
          setMetodoEditando(null);
        }}
        title={metodoEditando ? 'Editar método de pago' : 'Nuevo método de pago'}
        centered
      >
        <form onSubmit={form.onSubmit(handleSubmit)} className="flex flex-col gap-4">
          <TextInput label="Nombre" placeholder="Ej: Efectivo, Tarjeta de crédito" required {...form.getInputProps('nombre')} />

          <Select
            label="Modificador para el cliente"
            data={TIPO_MODIFICADOR_DATA}
            allowDeselect={false}
            {...form.getInputProps('tipoModificador')}
          />

          <NumberInput
            label="Porcentaje del modificador"
            description="Lo que se bonifica o recarga al precio que paga el cliente"
            min={0}
            max={100}
            decimalScale={2}
            suffix="%"
            disabled={form.values.tipoModificador === 'Ninguno'}
            {...form.getInputProps('porcentajeModificador')}
          />

          <NumberInput
            label="Comisión real del proveedor"
            description="Costo que te cobra el medio de pago (no afecta lo que paga el cliente, solo tu ganancia neta)"
            min={0}
            max={100}
            decimalScale={2}
            suffix="%"
            {...form.getInputProps('porcentajeComision')}
          />

          <Button type="submit" loading={submitting} fullWidth mt="sm">
            {metodoEditando ? 'Guardar cambios' : 'Crear método de pago'}
          </Button>
        </form>
      </Modal>
    </div>
  );
}
