using System;
using System.Collections.Generic;
using System.Text;
using TurnosApp.Core.Application.Exceptions;
using TurnosApp.Core.Application.Interfaces.Persistence;

namespace TurnosApp.Core.Application.Services;

/// <summary>
/// Domain Service en la capa Application.
/// Encapsula la regla de negocio de solapamiento de turnos.
/// Vive en Application (no en Domain) porque necesita consultar
/// el repositorio — el dominio puro no tiene acceso a persistencia.
/// </summary>
public class SolapamientoValidator
{
    private readonly ITurnoRepository _turnoRepository;

    public SolapamientoValidator(ITurnoRepository turnoRepository)
    {
        _turnoRepository = turnoRepository;
    }

    /// <summary>
    /// Valida que el rango [inicio, fin) esté libre para el recurso dado.
    /// Lanza <see cref="BusinessException"/> si existe solapamiento.
    /// </summary>
    /// <param name="recursoId">Recurso (profesional, sala, máquina) a validar.</param>
    /// <param name="inicio">Fecha y hora de inicio del turno propuesto.</param>
    /// <param name="fin">Fecha y hora de fin calculada (inicio + duración del servicio).</param>
    /// <param name="permitirSolapamiento">Flag del Tenant. Si es true, omite la validación.</param>
    public async Task ValidarAsync(
        int recursoId,
        DateTime inicio,
        DateTime fin,
        bool permitirSolapamiento,
        CancellationToken cancellationToken = default)
    {
        if (permitirSolapamiento)
            return;

        var existeSolapamiento = await _turnoRepository.ExisteTurnoEnRangoAsync(
            recursoId, inicio, fin, cancellationToken);

        if (existeSolapamiento)
        {
            throw new BusinessException(
                code: "TURNO_SOLAPADO",
                message: $"El recurso {recursoId} ya tiene un turno reservado " +
                         $"entre {inicio:dd/MM/yyyy HH:mm} y {fin:HH:mm}. " +
                         $"Por favor, seleccioná otro horario.");
        }
    }
}
