using System;
using System.Collections.Generic;
using System.Text;

namespace TurnosApp.Core.Domain.Exceptions;

public class SolapamientoException : DomainException
{
    public int RecursoId { get; }
    public DateTime InicioSolicitado { get; }
    public DateTime FinSolicitado { get; }

    public SolapamientoException(int recursoId, DateTime inicio, DateTime fin)
        : base($"El recurso {recursoId} ya tiene un turno entre {inicio:HH:mm} y {fin:HH:mm}.")
    {
        RecursoId = recursoId;
        InicioSolicitado = inicio;
        FinSolicitado = fin;
    }
}