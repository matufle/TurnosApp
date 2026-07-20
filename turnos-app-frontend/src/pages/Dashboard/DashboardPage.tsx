import { useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { format, isSameDay, isWithinInterval, startOfWeek, endOfWeek } from 'date-fns';
import { es } from 'date-fns/locale/es';
import { turnosService } from '../../api/turnosService';
import { tenantService } from '../../api/tenantService';
import type { Turno } from '../../types/Turno';

const DIAS_SEMANA = ['Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb', 'Dom'];

export function DashboardPage() {
  const navigate = useNavigate();
  const [turnos, setTurnos] = useState<Turno[]>([]);
  const [nombreNegocio, setNombreNegocio] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let activo = true;
    const cargar = async () => {
      try {
        const [turnosData, config] = await Promise.all([turnosService.getAll(), tenantService.getConfig()]);
        if (activo) {
          setTurnos(turnosData);
          setNombreNegocio(config.nombre ?? null);
        }
      } finally {
        if (activo) setLoading(false);
      }
    };
    cargar();
    return () => {
      activo = false;
    };
  }, []);

  const ahora = useMemo(() => new Date(), []);

  const turnosVigentes = useMemo(() => turnos.filter((t) => t.estado !== 'Cancelado'), [turnos]);

  const turnosHoy = useMemo(
    () => turnosVigentes.filter((t) => isSameDay(new Date(t.fechaHoraInicio), ahora)),
    [turnosVigentes, ahora]
  );

  const inicioSemana = useMemo(() => startOfWeek(ahora, { weekStartsOn: 1 }), [ahora]);
  const finSemana = useMemo(() => endOfWeek(ahora, { weekStartsOn: 1 }), [ahora]);

  const turnosSemana = useMemo(
    () =>
      turnosVigentes.filter((t) =>
        isWithinInterval(new Date(t.fechaHoraInicio), { start: inicioSemana, end: finSemana })
      ),
    [turnosVigentes, inicioSemana, finSemana]
  );

  const ingresosSemana = useMemo(
    () => turnosSemana.reduce((acc, t) => acc + t.precioTotal, 0),
    [turnosSemana]
  );

  const ingresosPorDia = useMemo(() => {
    const totales = new Array(7).fill(0);
    turnosSemana.forEach((t) => {
      // getDay(): 0=domingo..6=sábado -> lo convertimos a índice Lun=0..Dom=6
      const diaJs = new Date(t.fechaHoraInicio).getDay();
      const indice = diaJs === 0 ? 6 : diaJs - 1;
      totales[indice] += t.precioTotal;
    });
    return totales;
  }, [turnosSemana]);

  const maxIngresoDia = Math.max(...ingresosPorDia, 1);

  const proximasCitas = useMemo(
    () =>
      turnosVigentes
        .filter((t) => new Date(t.fechaHoraInicio) >= ahora)
        .sort((a, b) => new Date(a.fechaHoraInicio).getTime() - new Date(b.fechaHoraInicio).getTime())
        .slice(0, 4),
    [turnosVigentes, ahora]
  );

  const canceladosSemana = useMemo(
    () => turnos.filter((t) => t.estado === 'Cancelado' && isWithinInterval(new Date(t.fechaHoraInicio), { start: inicioSemana, end: finSemana })).length,
    [turnos, inicioSemana, finSemana]
  );

  if (loading) {
    return (
      <div className="flex items-center justify-center py-24">
        <span className="material-symbols-outlined animate-spin text-primary text-4xl">progress_activity</span>
      </div>
    );
  }

  return (
    <div className="flex flex-col gap-10 pb-12">
      {/* Encabezado */}
      <div className="flex flex-col md:flex-row justify-between items-start md:items-end gap-4">
        <div>
          <p className="text-secondary font-body-lg text-body-lg mb-1">Resumen general</p>
          <h1 className="font-display-lg text-display-lg text-on-surface">
            Hola{nombreNegocio ? `, ${nombreNegocio}` : ''}
          </h1>
        </div>
        <button
          onClick={() => navigate('/app/turnos')}
          className="flex items-center gap-2 bg-primary text-on-primary px-6 py-3 rounded-full font-title-md text-title-md hover:bg-primary-container transition-colors shadow-sm w-full md:w-auto justify-center"
        >
          <span className="material-symbols-outlined text-[20px]">add</span>
          Nuevo turno
        </button>
      </div>

      {/* Bento grid */}
      <div className="grid grid-cols-1 md:grid-cols-12 gap-gutter">
        {/* Turnos de hoy */}
        <div className="md:col-span-4 bg-surface-container-lowest rounded-2xl p-6 flex flex-col gap-4 shadow-[0px_4px_20px_rgba(0,188,212,0.08)]">
          <div className="flex justify-between items-start">
            <h2 className="font-title-md text-title-md text-on-surface">Turnos de hoy</h2>
            <span className="material-symbols-outlined text-primary-container p-2 bg-secondary-container rounded-full">
              today
            </span>
          </div>
          <div className="flex items-baseline gap-2 mt-2">
            <span className="font-display-lg text-display-lg text-primary">{turnosHoy.length}</span>
            <span className="font-body-sm text-body-sm text-secondary">
              {turnosHoy.length === 1 ? 'turno programado' : 'turnos programados'}
            </span>
          </div>
          <p className="mt-auto pt-4 font-body-sm text-body-sm text-secondary">
            {format(ahora, "EEEE d 'de' MMMM", { locale: es })}
          </p>
        </div>

        {/* Ingresos de la semana */}
        <div className="md:col-span-8 bg-surface-container-lowest rounded-2xl p-6 flex flex-col gap-4 shadow-[0px_4px_20px_rgba(0,188,212,0.08)]">
          <div className="flex justify-between items-start">
            <div>
              <h2 className="font-title-md text-title-md text-on-surface">Ingresos de la semana</h2>
              <span className="font-display-lg text-display-lg text-on-surface mt-2 block">
                ${ingresosSemana.toLocaleString('es-AR')}
              </span>
            </div>
            <span className="material-symbols-outlined text-primary-container p-2 bg-secondary-container rounded-full">
              payments
            </span>
          </div>

          <div className="mt-auto pt-6 flex items-end justify-between h-32 gap-2">
            {ingresosPorDia.map((total, i) => {
              const alturaPorcentaje = Math.max((total / maxIngresoDia) * 100, total > 0 ? 8 : 2);
              const esHoy = isSameDay(
                new Date(inicioSemana.getTime() + i * 24 * 60 * 60 * 1000),
                ahora
              );
              return (
                <div
                  key={DIAS_SEMANA[i]}
                  className={`w-full rounded-t-sm relative group transition-colors ${
                    esHoy ? 'bg-primary shadow-sm' : 'bg-surface-container hover:bg-primary-container'
                  }`}
                  style={{ height: `${alturaPorcentaje}%` }}
                >
                  <span className="absolute -top-8 left-1/2 -translate-x-1/2 opacity-0 group-hover:opacity-100 bg-inverse-surface text-inverse-on-surface text-xs py-1 px-2 rounded transition-opacity whitespace-nowrap">
                    {DIAS_SEMANA[i]} · ${total.toLocaleString('es-AR')}
                  </span>
                </div>
              );
            })}
          </div>
          <div className="flex justify-between px-0.5">
            {DIAS_SEMANA.map((dia) => (
              <span key={dia} className="text-xs text-secondary flex-1 text-center">
                {dia}
              </span>
            ))}
          </div>
        </div>

        {/* Próximas citas */}
        <div className="md:col-span-6 bg-surface-container-lowest rounded-2xl p-6 flex flex-col gap-6 shadow-[0px_4px_20px_rgba(0,188,212,0.08)]">
          <div className="flex justify-between items-center border-b border-surface-container pb-4">
            <h2 className="font-title-md text-title-md text-on-surface">Próximas citas</h2>
            <button
              onClick={() => navigate('/app/turnos')}
              className="text-primary font-body-sm text-body-sm hover:underline"
            >
              Ver todas
            </button>
          </div>

          {proximasCitas.length === 0 ? (
            <p className="font-body-sm text-body-sm text-secondary py-4">No tenés turnos próximos.</p>
          ) : (
            <div className="flex flex-col gap-4">
              {proximasCitas.map((turno) => {
                const fecha = new Date(turno.fechaHoraInicio);
                return (
                  <div
                    key={turno.id}
                    className="flex items-center p-3 rounded-lg hover:bg-surface-bright transition-colors border-l-4 border-primary bg-surface-container-low gap-4"
                  >
                    <div className="text-center min-w-[64px]">
                      <p className="font-title-md text-title-md text-primary">{format(fecha, 'HH:mm')}</p>
                      <p className="font-body-sm text-body-sm text-secondary">{format(fecha, 'd MMM', { locale: es })}</p>
                    </div>
                    <div className="flex-grow min-w-0">
                      <p className="font-title-md text-title-md text-on-surface truncate">
                        {turno.clienteNombreCompleto}
                      </p>
                      <p className="font-body-sm text-body-sm text-secondary truncate">
                        {turno.servicios.join(', ')} · {turno.recursoNombre}
                      </p>
                    </div>
                  </div>
                );
              })}
            </div>
          )}
        </div>

        {/* Resumen de la semana */}
        <div className="md:col-span-6 bg-surface-container-lowest rounded-2xl p-6 flex flex-col gap-6 shadow-[0px_4px_20px_rgba(0,188,212,0.08)]">
          <div className="flex justify-between items-center border-b border-surface-container pb-4">
            <h2 className="font-title-md text-title-md text-on-surface">Resumen de la semana</h2>
          </div>

          <div className="flex flex-col gap-5">
            <div className="flex gap-4 items-center">
              <div className="bg-secondary-container text-on-secondary-container p-2 rounded-full shrink-0">
                <span className="material-symbols-outlined text-[20px]">event_available</span>
              </div>
              <div className="flex-grow">
                <p className="font-body-lg text-body-lg text-on-surface">Turnos confirmados</p>
                <p className="font-body-sm text-body-sm text-secondary">Lunes a domingo</p>
              </div>
              <div className="bg-[#e6f4ea] text-[#137333] px-3 py-1 rounded-full font-label-md text-label-md">
                {turnosSemana.length}
              </div>
            </div>

            <div className="flex gap-4 items-center">
              <div className="bg-surface-container text-secondary p-2 rounded-full shrink-0">
                <span className="material-symbols-outlined text-[20px]">event_busy</span>
              </div>
              <div className="flex-grow">
                <p className="font-body-lg text-body-lg text-on-surface">Turnos cancelados</p>
                <p className="font-body-sm text-body-sm text-secondary">Lunes a domingo</p>
              </div>
              <div className="bg-surface-container text-secondary px-3 py-1 rounded-full font-label-md text-label-md">
                {canceladosSemana}
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
