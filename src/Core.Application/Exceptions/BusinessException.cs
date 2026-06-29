using System;
using System.Collections.Generic;
using System.Text;
namespace TurnosApp.Core.Application.Exceptions;

/// <summary>
/// Representa una violación de regla de negocio que requiere
/// consultar estado externo (base de datos, servicios externos).
/// A diferencia de DomainException, no es una invariante del modelo
/// sino una restricción de proceso.
/// El middleware de la WebAPI la traducirá a HTTP 409 Conflict.
/// </summary>
public class BusinessException : Exception
{
    public string Code { get; }

    public BusinessException(string code, string message) : base(message)
    {
        Code = code;
    }
}