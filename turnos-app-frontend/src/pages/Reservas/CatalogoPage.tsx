// src/pages/Reservas/CatalogoPage.tsx
// Ruta índice de /reservas/:slug — catálogo público de solo lectura (anónimo).
import { useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { publicCatalogoService } from '../../api/publicCatalogoService';
import { PageSpinner } from '../../components/PageSpinner';
import type { Servicio } from '../../types/Servicio';

const ACENTOS = ['bg-primary-container', 'bg-primary', 'bg-secondary', 'bg-tertiary-container'];

export function CatalogoPage() {
  const { slug } = useParams<{ slug: string }>();
  const [servicios, setServicios] = useState<Servicio[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!slug) return;
    publicCatalogoService.getServicios(slug).then((data) => {
      setServicios(data);
      setLoading(false);
    });
  }, [slug]);

  if (loading) {
    return <PageSpinner />;
  }

  return (
    <div className="max-w-[800px] w-full mx-auto px-margin-mobile md:px-margin-desktop py-12">
      <div className="mb-12 text-center md:text-left">
        <h1 className="font-headline-lg text-headline-lg-mobile md:text-headline-lg text-on-surface mb-2">
          Elegir servicio
        </h1>
        <p className="font-body-lg text-body-lg text-on-surface-variant">
          Seleccioná el servicio que querés reservar hoy.
        </p>
      </div>

      {servicios.length === 0 ? (
        <p className="font-body-lg text-body-lg text-on-surface-variant text-center">
          Todavía no hay servicios disponibles.
        </p>
      ) : (
        <div className="space-y-6">
          {servicios.map((servicio, index) => (
            <div
              key={servicio.id}
              className="bg-surface-container-lowest soft-elevation rounded-[32px] p-6 md:p-8 flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 border border-transparent hover:border-primary-container/20 transition-all group"
            >
              <div className="flex items-center gap-6">
                <div className={`w-2 h-12 rounded-full flex-shrink-0 ${ACENTOS[index % ACENTOS.length]}`} />
                <div>
                  <h3 className="font-title-md text-title-md text-on-surface group-hover:text-primary transition-colors">
                    {servicio.nombre}
                  </h3>
                  <p className="font-body-sm text-body-sm text-outline">
                    {servicio.duracionMinutos} min — ${servicio.precio.toLocaleString('es-AR')}
                  </p>
                </div>
              </div>

              <Link
                to={`/reservas/${slug}/reservar?servicioId=${servicio.id}`}
                className="no-underline bg-primary-container hover:brightness-95 text-on-primary font-label-md text-label-md py-3 px-8 rounded-xl transition-all shadow-md active:scale-95 w-full sm:w-auto text-center"
              >
                Reservar
              </Link>
            </div>
          ))}
        </div>
      )}

      <div className="mt-12 text-center">
        <p className="font-body-lg text-body-lg text-on-surface-variant">
          ¿Ya reservaste antes?{' '}
          <Link
            to={`/reservas/${slug}/mis-turnos`}
            className="text-primary font-semibold hover:underline decoration-2 underline-offset-4 ml-1 transition-all"
          >
            Ver mis turnos
          </Link>
        </p>
      </div>
    </div>
  );
}
