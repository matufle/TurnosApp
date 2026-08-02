import type { ReactNode } from 'react';
import { Link } from 'react-router-dom';

interface LegalPageLayoutProps {
  title: string;
  actualizadoEl: string;
  children: ReactNode;
}

export function LegalPageLayout({ title, actualizadoEl, children }: LegalPageLayoutProps) {
  return (
    <div className="bg-surface-bright text-on-surface font-body-lg text-body-lg min-h-screen">
      <header className="w-full border-b border-surface-variant/30">
        <div className="flex items-center justify-between px-margin-mobile md:px-margin-desktop w-full max-w-container-max mx-auto h-20">
          <Link to="/" className="flex items-center gap-2 no-underline">
            <span className="material-symbols-outlined text-primary text-3xl" style={{ fontVariationSettings: "'FILL' 1" }}>
              calendar_today
            </span>
            <span className="font-headline-lg-mobile text-headline-lg-mobile md:font-headline-lg md:text-headline-lg text-primary tracking-tight">
              Slotia
            </span>
          </Link>
          <Link
            to="/"
            className="font-body-sm text-body-sm text-on-surface-variant hover:text-primary no-underline hover:underline underline-offset-4"
          >
            Volver al inicio
          </Link>
        </div>
      </header>

      <main className="px-margin-mobile md:px-margin-desktop py-12">
        <div className="w-full max-w-[760px] mx-auto">
          <div className="bg-surface-container-lowest rounded-3xl p-8 md:p-12 soft-elevation border border-surface-variant/30">
            <h1 className="font-headline-lg text-headline-lg text-on-surface mb-2">{title}</h1>
            <p className="font-body-sm text-body-sm text-on-surface-variant mb-8">Última actualización: {actualizadoEl}</p>

            <div className="space-y-6 [&_h2]:font-title-md [&_h2]:text-title-md [&_h2]:text-on-surface [&_h2]:mt-8 [&_h2]:mb-2 [&_h2]:font-semibold [&_p]:font-body-sm [&_p]:text-body-sm [&_p]:text-on-surface-variant [&_p]:leading-relaxed [&_li]:font-body-sm [&_li]:text-body-sm [&_li]:text-on-surface-variant [&_ul]:list-disc [&_ul]:pl-6 [&_ul]:space-y-1 [&_a]:text-primary [&_a]:font-semibold [&_a]:hover:underline">
              {children}
            </div>
          </div>
        </div>
      </main>
    </div>
  );
}
