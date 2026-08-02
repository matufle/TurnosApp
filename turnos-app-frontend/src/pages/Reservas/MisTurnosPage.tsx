// src/pages/Reservas/MisTurnosPage.tsx
import { useCallback, useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { ActionIcon, Menu } from '@mantine/core';
import { IconDots } from '@tabler/icons-react';
import { misTurnosService } from '../../api/misTurnosService';
import { useClienteAuth } from '../../context/useClienteAuth';
import { PageSpinner } from '../../components/PageSpinner';
import type { Turno } from '../../types/Turno';

const LABEL_POR_ESTADO: Record<string, string> = {
  Pendiente: 'Pendiente',
  Confirmado: 'Confirmado',
  EnCurso: 'En curso',
  Completado: 'Completado',
  Cancelado: 'Cancelado',
  Ausente: 'Ausente',
};

const BADGE_POR_ESTADO: Record<string, string> = {
  Pendiente: 'bg-secondary-container text-on-secondary-container',
  Confirmado: 'bg-primary/10 text-primary',
  EnCurso: 'bg-tertiary/10 text-tertiary',
  Completado: 'bg-surface-container text-on-surface-variant',
  Cancelado: 'bg-error/10 text-error',
  Ausente: 'bg-error-container text-on-error-container',
};

const ESTADOS_ABIERTOS = ['Pendiente', 'Confirmado', 'EnCurso'];

function accentPorEstado(estado: string): string {
  return ESTADOS_ABIERTOS.includes(estado) ? 'bg-primary' : 'bg-outline-variant';
}

function formatFechaCorta(fecha: Date): string {
  const dia = fecha.toLocaleDateString('es-AR', { day: '2-digit' });
  const mes = fecha.toLocaleDateString('es-AR', { month: 'short' }).replace('.', '');
  const anio = fecha.toLocaleDateString('es-AR', { year: 'numeric' });
  return `${dia} ${mes.charAt(0).toUpperCase()}${mes.slice(1)}, ${anio}`;
}

export function MisTurnosPage() {
  const { slug } = useParams<{ slug: string }>();
  const navigate = useNavigate();
  const { logout } = useClienteAuth();
  const [turnos, setTurnos] = useState<Turno[]>([]);
  const [loading, setLoading] = useState(true);
  const [cancelandoId, setCancelandoId] = useState<number | null>(null);

  const cargarTurnos = useCallback(async () => {
    setLoading(true);
    try {
      const data = await misTurnosService.getMisTurnos();
      setTurnos(data);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    cargarTurnos();
  }, [cargarTurnos]);

  const handleCancelar = async (id: number) => {
    setCancelandoId(id);
    try {
      await misTurnosService.cancelar(id);
      await cargarTurnos();
    } finally {
      setCancelandoId(null);
    }
  };

  const handleLogout = () => {
    logout();
    navigate(`/reservas/${slug}/login`, { replace: true });
  };

  if (loading) {
    return <PageSpinner />;
  }

  return (
    <div className="max-w-container-max mx-auto px-margin-mobile md:px-margin-desktop py-8 pb-16">
      <div className="flex flex-col md:flex-row md:items-end justify-between gap-6 mb-10">
        <div>
          <h1 className="font-headline-lg text-headline-lg-mobile md:text-headline-lg text-on-surface mb-2">
            Mis turnos
          </h1>
          <p className="text-on-surface-variant font-body-lg text-body-lg">
            Gestioná tus citas y reservá nuevos servicios fácilmente.
          </p>
        </div>
        <div className="flex flex-col md:flex-row gap-3 w-full md:w-auto">
          <button
            type="button"
            onClick={handleLogout}
            className="font-label-md text-label-md text-on-surface-variant hover:text-error transition-colors flex items-center justify-center gap-1 py-2"
          >
            <span className="material-symbols-outlined text-[18px]">logout</span>
            Cerrar sesión
          </button>
          <Link
            to={`/reservas/${slug}`}
            className="no-underline bg-primary-container text-on-primary-container font-title-md text-title-md py-4 px-8 rounded-xl soft-elevation hover:brightness-105 active:scale-95 transition-all flex items-center justify-center gap-2 w-full md:w-auto"
          >
            <span className="material-symbols-outlined">add</span>
            Reservar otro turno
          </Link>
        </div>
      </div>

      {turnos.length === 0 ? (
        <section className="flex flex-col items-center justify-center py-20 text-center">
          <div className="w-24 h-24 bg-surface-container rounded-full flex items-center justify-center mb-6">
            <span className="material-symbols-outlined text-primary text-4xl">event_busy</span>
          </div>
          <h2 className="font-title-md text-title-md text-on-surface mb-2">Sin turnos pendientes</h2>
          <p className="text-on-surface-variant font-body-lg text-body-lg">Todavía no tenés turnos.</p>
        </section>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-gutter">
          {turnos.map((turno) => {
            const abierto = turno.estado !== 'Cancelado';
            const fecha = new Date(turno.fechaHoraInicio);

            return (
              <div
                key={turno.id}
                className={`bg-surface-container-lowest rounded-[32px] p-6 soft-elevation border border-surface-container relative overflow-hidden ${
                  abierto ? 'transition-transform hover:-translate-y-1' : 'opacity-80'
                }`}
              >
                <div className={`absolute left-0 top-0 bottom-0 w-2 ${accentPorEstado(turno.estado)}`} />

                <div className="flex justify-between items-start mb-4">
                  <span
                    className={`font-label-md text-label-md px-3 py-1 rounded-full uppercase tracking-wider ${
                      BADGE_POR_ESTADO[turno.estado] ?? BADGE_POR_ESTADO.Completado
                    }`}
                  >
                    {LABEL_POR_ESTADO[turno.estado] ?? turno.estado}
                  </span>

                  {abierto && (
                    <Menu shadow="md" position="bottom-end" withinPortal>
                      <Menu.Target>
                        <ActionIcon variant="subtle" color="gray" aria-label="Más opciones">
                          <IconDots size={18} />
                        </ActionIcon>
                      </Menu.Target>
                      <Menu.Dropdown>
                        <Menu.Item
                          color="red"
                          disabled={cancelandoId === turno.id}
                          onClick={() => handleCancelar(turno.id)}
                        >
                          {cancelandoId === turno.id ? 'Cancelando...' : 'Cancelar turno'}
                        </Menu.Item>
                      </Menu.Dropdown>
                    </Menu>
                  )}
                </div>

                <h3 className="font-title-md text-title-md text-on-surface mb-1">{turno.servicios.join(', ')}</h3>
                <p className="text-on-surface-variant font-body-sm text-body-sm mb-4 flex items-center gap-1">
                  <span className="material-symbols-outlined text-sm">person</span>
                  Prof: {turno.recursoNombre}
                </p>

                <div className="flex items-center gap-4 text-on-surface pt-4 border-t border-surface-container">
                  <div className="flex items-center gap-2">
                    <span className={`material-symbols-outlined ${abierto ? 'text-primary' : 'text-outline'}`}>
                      calendar_today
                    </span>
                    <span className="font-label-md text-label-md">{formatFechaCorta(fecha)}</span>
                  </div>
                  <div className="flex items-center gap-2">
                    <span className={`material-symbols-outlined ${abierto ? 'text-primary' : 'text-outline'}`}>
                      schedule
                    </span>
                    <span className="font-label-md text-label-md">
                      {fecha.toLocaleTimeString('es-AR', { hour: '2-digit', minute: '2-digit' })} hs
                    </span>
                  </div>
                </div>
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}
