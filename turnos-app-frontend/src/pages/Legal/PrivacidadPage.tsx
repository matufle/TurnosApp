import { LegalPageLayout } from './LegalPageLayout';

export function PrivacidadPage() {
  return (
    <LegalPageLayout title="Política de Privacidad" actualizadoEl="1 de agosto de 2026">
      <p>
        Esta Política de Privacidad describe qué datos personales recolecta Slotia, cómo los usamos y qué derechos
        tenés sobre ellos, en línea con la Ley 25.326 de Protección de Datos Personales de la República Argentina.
      </p>

      <h2>1. Qué datos recolectamos</h2>
      <p>Según cómo uses Slotia, podemos recolectar:</p>
      <ul>
        <li>
          <strong>Datos de cuenta de negocio:</strong> nombre del negocio, email y contraseña (almacenada siempre
          como hash, nunca en texto plano) de quienes administran o trabajan en el negocio.
        </li>
        <li>
          <strong>Datos de clientes finales:</strong> nombre, apellido, email y teléfono, ya sea cargados por el
          negocio o ingresados por el propio cliente al crear su cuenta de reserva.
        </li>
        <li>
          <strong>Datos de uso:</strong> turnos, servicios, cobros y demás información operativa que el negocio
          carga para gestionar su actividad.
        </li>
        <li>
          <strong>Datos técnicos:</strong> dirección IP, tipo de dispositivo/navegador y datos de interacción con la
          aplicación, recolectados mediante las herramientas descriptas en la sección 3.
        </li>
      </ul>

      <h2>2. Para qué usamos tus datos</h2>
      <ul>
        <li>Prestar y mantener el Servicio (crear turnos, enviar confirmaciones, procesar cobros).</li>
        <li>Enviar emails transaccionales: confirmación de cuenta, confirmación y recordatorio de turnos, avisos de lista de espera.</li>
        <li>Procesar el pago de la suscripción a través de Mercado Pago.</li>
        <li>Detectar y prevenir errores, abuso o uso fraudulento del Servicio.</li>
        <li>Mejorar el Servicio a partir de datos de uso agregados.</li>
      </ul>
      <p>No vendemos datos personales a terceros ni los usamos para publicidad de terceros.</p>

      <h2>3. Servicios de terceros que usamos</h2>
      <p>Slotia se apoya en los siguientes proveedores, cada uno con su propia política de privacidad:</p>
      <ul>
        <li>
          <strong>Mercado Pago:</strong> procesa el pago de la suscripción del negocio. No almacenamos datos de
          tarjetas: esa información queda exclusivamente en manos de Mercado Pago.
        </li>
        <li>
          <strong>Sentry:</strong> reporte de errores técnicos, para detectar y corregir fallas del Servicio.
        </li>
        <li>
          <strong>Mixpanel:</strong> analítica de producto sobre un conjunto acotado de eventos clave (registro,
          login, turno creado, suscripción), para entender el uso general del Servicio.
        </li>
        <li>
          <strong>Hotjar (Contentsquare):</strong> grabación de sesiones y mapas de calor sobre el uso de la
          interfaz, para mejorar la experiencia de uso.
        </li>
      </ul>

      <h2>4. Cómo almacenamos y protegemos tus datos</h2>
      <p>
        Los datos se almacenan en una base de datos PostgreSQL con acceso restringido. Las contraseñas se guardan
        siempre hasheadas, nunca en texto plano. El acceso a la aplicación requiere autenticación mediante tokens
        (JWT) y cada negocio solo puede acceder a sus propios datos (aislamiento por tenant).
      </p>

      <h2>5. Cuánto tiempo conservamos los datos</h2>
      <p>
        Conservamos tus datos mientras tu cuenta esté activa. Si cancelás tu cuenta, podés solicitarnos la
        eliminación de tus datos; podremos conservar cierta información cuando así lo exija una obligación legal
        (por ejemplo, registros vinculados a facturación).
      </p>

      <h2>6. Tus derechos</h2>
      <p>
        De acuerdo con la Ley 25.326, tenés derecho a acceder, rectificar, actualizar y solicitar la supresión de
        tus datos personales. También podés retirar tu consentimiento en cualquier momento. Para ejercer estos
        derechos, escribinos a <a href="mailto:soporte@slotia.app">soporte@slotia.app</a>. La Agencia de Acceso a la
        Información Pública, en su carácter de Órgano de Control de la Ley 25.326, tiene la atribución de atender las
        denuncias y reclamos que se interpongan con relación al incumplimiento de las normas sobre protección de
        datos personales.
      </p>

      <h2>7. Cambios a esta política</h2>
      <p>
        Podemos actualizar esta Política de Privacidad ocasionalmente. Si el cambio es significativo, te
        avisaremos por email o dentro de la aplicación.
      </p>

      <h2>8. Contacto</h2>
      <p>
        Ante cualquier consulta sobre esta política o tus datos personales, escribinos a{' '}
        <a href="mailto:soporte@slotia.app">soporte@slotia.app</a>.
      </p>
    </LegalPageLayout>
  );
}
