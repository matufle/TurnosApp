import { LegalPageLayout } from './LegalPageLayout';

export function TerminosPage() {
  return (
    <LegalPageLayout title="Términos de Servicio" actualizadoEl="1 de agosto de 2026">
      <p>
        Estos Términos de Servicio ("Términos") regulan el uso de Slotia (el "Servicio"), una plataforma de gestión de
        turnos y reservas para negocios. Al crear una cuenta o usar el Servicio, aceptás estos Términos. Si no estás de
        acuerdo, no debés usar Slotia.
      </p>

      <h2>1. Descripción del Servicio</h2>
      <p>
        Slotia permite a un negocio ("Cuenta de Negocio" o "vos") gestionar recursos, servicios, turnos, clientes y
        cobros, y opcionalmente habilitar que sus propios clientes finales reserven turnos por su cuenta a través de
        una página pública. Nos reservamos el derecho de agregar, modificar o discontinuar funcionalidades del
        Servicio en cualquier momento.
      </p>

      <h2>2. Registro y cuentas</h2>
      <p>
        Para usar Slotia necesitás crear una cuenta con un email válido y confirmar ese email. Sos responsable de
        mantener la confidencialidad de tu contraseña y de toda actividad que ocurra bajo tu cuenta. Debés
        notificarnos de inmediato ante cualquier uso no autorizado.
      </p>
      <p>
        Si tu negocio habilita la reserva pública, tus propios clientes finales podrán crear su propia cuenta de
        cliente dentro de tu espacio para reservar y gestionar sus turnos. Esos datos son administrados por vos como
        responsable del negocio, sujeto a nuestra{' '}
        <a href="/privacidad">Política de Privacidad</a>.
      </p>

      <h2>3. Planes, precios y facturación</h2>
      <p>
        Slotia ofrece un período de prueba gratuito. Al finalizar el período de prueba, continuar usando el Servicio
        requiere una suscripción paga con renovación mensual automática, procesada a través de Mercado Pago. El
        precio vigente se muestra en la aplicación antes de confirmar la suscripción y puede modificarse con aviso
        previo razonable.
      </p>
      <p>
        Podés cancelar tu suscripción en cualquier momento desde la sección "Suscripción" de tu panel. La
        cancelación tiene efecto a partir del próximo ciclo de facturación; no se realizan reembolsos proporcionales
        por el período ya facturado, salvo que la ley aplicable indique lo contrario.
      </p>
      <p>
        Si tu suscripción queda inactiva (pago rechazado, cancelación, o vencimiento del período de prueba sin
        suscribirte), podemos restringir el acceso a funcionalidades del Servicio hasta que regularices tu
        situación. Tus datos no se eliminan solo por esto.
      </p>

      <h2>4. Uso aceptable</h2>
      <p>Al usar Slotia, te comprometés a no:</p>
      <ul>
        <li>Usar el Servicio para fines ilegales o no autorizados.</li>
        <li>Intentar acceder a datos de otro negocio (tenant) distinto del tuyo.</li>
        <li>Interferir con el funcionamiento normal del Servicio (ataques, scraping abusivo, sobrecarga intencional).</li>
        <li>Cargar contenido difamatorio, fraudulento o que infrinja derechos de terceros.</li>
      </ul>
      <p>Nos reservamos el derecho de suspender o cerrar cuentas que incumplan estas reglas.</p>

      <h2>5. Propiedad de los datos</h2>
      <p>
        Los datos que cargás en Slotia (clientes, turnos, servicios, cobros, etc.) son tuyos. Los usamos únicamente
        para prestarte el Servicio y no los vendemos a terceros. Podés solicitar una exportación o eliminación de tus
        datos escribiéndonos, sujeto a obligaciones legales de conservación que pudieran aplicar (por ejemplo,
        registros de facturación).
      </p>

      <h2>6. Disponibilidad del Servicio</h2>
      <p>
        Hacemos nuestro mejor esfuerzo para mantener Slotia disponible de forma continua, pero no garantizamos un
        nivel de servicio (SLA) formal. Puede haber interrupciones programadas o no programadas por mantenimiento,
        fallas técnicas, o causas fuera de nuestro control.
      </p>

      <h2>7. Limitación de responsabilidad</h2>
      <p>
        Slotia se provee "tal cual". En la medida permitida por la ley aplicable, no somos responsables por daños
        indirectos, lucro cesante, o pérdida de datos derivados del uso o la imposibilidad de uso del Servicio. Nada
        en estos Términos limita responsabilidad que no pueda limitarse por ley.
      </p>

      <h2>8. Modificaciones a estos Términos</h2>
      <p>
        Podemos actualizar estos Términos ocasionalmente. Si el cambio es significativo, te avisaremos por email o
        dentro de la aplicación. El uso continuado del Servicio después de una actualización implica tu aceptación de
        los nuevos Términos.
      </p>

      <h2>9. Legislación aplicable</h2>
      <p>
        Estos Términos se rigen por las leyes de la República Argentina. Cualquier disputa se someterá a los
        tribunales competentes de Argentina, sin perjuicio de los derechos que la normativa de defensa del
        consumidor pudiera reconocerte según tu jurisdicción.
      </p>

      <h2>10. Contacto</h2>
      <p>
        Ante cualquier consulta sobre estos Términos, escribinos a{' '}
        <a href="mailto:soporte@slotia.app">soporte@slotia.app</a>.
      </p>
    </LegalPageLayout>
  );
}
