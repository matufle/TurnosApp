using System;
using System.Collections.Generic;
using System.Text;

// La separo de BadRequestException: un duplicado es semánticamente un 409, no un 400.
namespace TurnosApp.Core.Exceptions;

public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}
