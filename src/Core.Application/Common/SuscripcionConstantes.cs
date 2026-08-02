namespace TurnosApp.Core.Application.Common;

public static class SuscripcionConstantes
{
    // Días de gracia tras entrar en PastDue (falló el cobro) antes de bloquear el acceso
    // del tenant — le da tiempo a actualizar el método de pago sin cortarlo de un día para el otro.
    public const int DiasGraciaPastDue = 20;
}
