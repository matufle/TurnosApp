using System;
using System.Collections.Generic;
using System.Text;

namespace TurnosApp.Core.Application.Exceptions;

/// <summary>
/// Se lanza cuando se solicita un recurso por ID y no existe
/// (o no pertenece al tenant actual, gracias al Global Query Filter).
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string entityName, object key)
        : base($"'{entityName}' con id '{key}' no fue encontrado.")
    {
    }
}