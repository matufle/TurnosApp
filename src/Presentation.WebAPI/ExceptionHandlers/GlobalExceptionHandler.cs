using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TurnosApp.Core.Application.Exceptions;
using TurnosApp.Core.Domain.Exceptions;

namespace TurnosApp.Presentation.WebAPI.ExceptionHandlers;

/// <summary>
/// Centraliza la traducción de excepciones de dominio/aplicación
/// a respuestas HTTP con ProblemDetails (RFC 7807).
/// Registrado en DI como IExceptionHandler (disponible desde .NET 8).
/// Evita try/catch en cada controller.
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title, detail, extensions) = exception switch
        {
            BusinessException bex => (
                StatusCodes.Status409Conflict,
                "Conflicto de negocio",
                bex.Message,
                (IDictionary<string, object?>?)new Dictionary<string, object?> { ["code"] = bex.Code }
            ),

            NotFoundException nex => (
                StatusCodes.Status404NotFound,
                "Recurso no encontrado",
                nex.Message,
                (IDictionary<string, object?>?)null
            ),

            DomainException dex => (
                StatusCodes.Status422UnprocessableEntity,
                "Violación de regla de dominio",
                dex.Message,
                (IDictionary<string, object?>?)null
            ),

            _ => (
                StatusCodes.Status500InternalServerError,
                "Error interno del servidor",
                "Ocurrió un error inesperado. Por favor, intentá más tarde.",
                (IDictionary<string, object?>?)null
            )
        };

        // Logueamos el stack completo solo para errores no controlados.
        if (statusCode == StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Excepción no controlada: {Message}", exception.Message);
        else
            _logger.LogWarning(exception, "Excepción de negocio: {Message}", exception.Message);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        if (extensions is not null)
            foreach (var (key, value) in extensions)
                problemDetails.Extensions[key] = value;

        context.Response.StatusCode = statusCode;

        await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true; // true = excepción manejada, el pipeline no propaga más.
    }
}