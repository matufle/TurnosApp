using System;
using System.Collections.Generic;
using System.Text;

// Separada de las demás: un 403 significa "autenticado pero sin permiso", no "no autenticado" (401)
// ni una regla de negocio violada (409).
namespace TurnosApp.Core.Exceptions;

public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message) { }
}
