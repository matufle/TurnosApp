using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sentry;
using TurnosApp.Core.Application.Exceptions; // BusinessException
using TurnosApp.Core.Domain.Exceptions;
using TurnosApp.Core.Exceptions;      // NotFoundException, BadRequestException, ConflictException, SolapamientoException, DomainException

namespace TurnosApp.Presentation.WebAPI.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title, detail) = MapException(exception);

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Excepción no controlada: {Message}", exception.Message);
            CapturarEnSentry(httpContext, exception);
        }
        else
            _logger.LogWarning("Excepción de negocio ({Status}): {Message}", statusCode, detail);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

        if (exception is BusinessException businessException)
            problemDetails.Extensions["code"] = businessException.Code;

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private (int StatusCode, string Title, string Detail) MapException(Exception exception) => exception switch
    {
        NotFoundException => (StatusCodes.Status404NotFound, "Recurso no encontrado", exception.Message),

        SolapamientoException => (StatusCodes.Status409Conflict, "Conflicto de horario", exception.Message),

        ConflictException => (StatusCodes.Status409Conflict, "Conflicto con el estado actual del recurso", exception.Message),

        BusinessException => (StatusCodes.Status409Conflict, "Regla de negocio violada", exception.Message),

        BadRequestException => (StatusCodes.Status400BadRequest, "Solicitud inválida", exception.Message),

        DomainException => (StatusCodes.Status422UnprocessableEntity, "Violación de una regla del dominio", exception.Message),

        UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "No autorizado", exception.Message),

        ForbiddenException => (StatusCodes.Status403Forbidden, "Acceso denegado", exception.Message),

        DbUpdateException dbEx => (
            StatusCodes.Status409Conflict,
            "No se pudo guardar el cambio",
            _env.IsDevelopment()
                ? $"{dbEx.Message} | Inner: {dbEx.InnerException?.Message}"
                : "Los datos ingresados violan una restricción existente (posible duplicado o referencia inválida)."
        ),

        _ => (
            StatusCodes.Status500InternalServerError,
            "Ocurrió un error interno inesperado",
            _env.IsDevelopment() ? exception.Message : "Contactá al administrador si el problema persiste."
        )
    };

    // Solo 500s no mapeados: los 4xx de negocio (BusinessException, NotFoundException, etc.)
    // son ruido esperado del día a día, no bugs — reportarlos inundaría Sentry sin agregar valor.
    private static void CapturarEnSentry(HttpContext httpContext, Exception exception)
    {
        SentrySdk.CaptureException(exception, scope =>
        {
            var tenantId = httpContext.User.FindFirst("TenantId")?.Value;
            var usuarioId = httpContext.User.FindFirst("UsuarioId")?.Value;

            if (!string.IsNullOrEmpty(tenantId))
                scope.SetTag("tenantId", tenantId);

            if (!string.IsNullOrEmpty(usuarioId))
                scope.User = new SentryUser { Id = usuarioId };
        });
    }
}