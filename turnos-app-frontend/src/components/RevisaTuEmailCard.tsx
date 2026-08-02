// src/components/RevisaTuEmailCard.tsx
import { Link } from 'react-router-dom';

interface RevisaTuEmailCardProps {
  email: string;
  loginHref: string;
  reenviando: boolean;
  reenviado: boolean;
  onReenviar: () => void;
  onCambiarEmail: () => void;
}

export function RevisaTuEmailCard({
  email,
  loginHref,
  reenviando,
  reenviado,
  onReenviar,
  onCambiarEmail,
}: RevisaTuEmailCardProps) {
  return (
    <div className="max-w-md w-full mx-auto bg-surface-container-lowest rounded-[32px] p-8 md:p-12 soft-elevation border border-outline-variant/20">
      <div className="flex flex-col items-center text-center">
        <div className="w-24 h-24 bg-primary-container/10 rounded-full flex items-center justify-center mb-8">
          <span
            className="material-symbols-outlined text-primary text-[48px]"
            style={{ fontVariationSettings: "'FILL' 1" }}
          >
            mark_email_read
          </span>
        </div>

        <h1 className="font-headline-lg-mobile md:font-headline-lg text-headline-lg-mobile md:text-headline-lg text-on-surface mb-4">
          ¡Casi listo! Revisá tu bandeja de entrada
        </h1>

        <p className="font-body-lg text-body-lg text-on-surface-variant mb-8">
          Te enviamos un enlace de confirmación a <strong className="text-on-surface">{email}</strong>. Hacé clic en
          el enlace para activar tu cuenta.
        </p>

        <div className="bg-surface-container-low p-4 rounded-xl mb-8 w-full">
          <p className="font-body-sm text-body-sm text-on-surface-variant flex items-center justify-center gap-2">
            <span className="material-symbols-outlined text-[18px]">info</span>
            ¿No lo encontrás? Revisá tu carpeta de spam.
          </p>
        </div>

        {reenviado ? (
          <div className="w-full bg-primary/10 text-primary font-title-md text-title-md py-4 px-6 rounded-full mb-6 flex items-center justify-center gap-2">
            <span className="material-symbols-outlined">check_circle</span>
            ¡Correo enviado!
          </div>
        ) : (
          <button
            type="button"
            onClick={onReenviar}
            disabled={reenviando}
            className="w-full bg-primary-container hover:bg-primary disabled:opacity-70 disabled:cursor-not-allowed text-on-primary font-title-md text-title-md py-4 px-6 rounded-full soft-elevation transition-all duration-300 active:scale-[0.98] mb-6 flex items-center justify-center gap-2"
          >
            <span className={`material-symbols-outlined ${reenviando ? 'animate-spin' : ''}`}>
              {reenviando ? 'progress_activity' : 'send'}
            </span>
            {reenviando ? 'Enviando...' : 'Reenviar correo'}
          </button>
        )}

        <div className="flex flex-col gap-4">
          <button
            type="button"
            onClick={onCambiarEmail}
            className="font-body-sm text-body-sm text-primary hover:text-on-primary-container font-semibold transition-colors"
          >
            Cambiar correo
          </button>
          <Link
            to={loginHref}
            className="font-body-sm text-body-sm text-on-surface-variant hover:text-on-surface transition-colors no-underline"
          >
            Ir a iniciar sesión
          </Link>
        </div>
      </div>
    </div>
  );
}
