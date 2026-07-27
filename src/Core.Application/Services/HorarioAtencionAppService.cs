using TurnosApp.Core.Application.DTOs.Horarios;
using TurnosApp.Core.Application.Interfaces.Persistence;
using TurnosApp.Core.Application.Interfaces.Services;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Core.Exceptions;

namespace TurnosApp.Core.Application.Services;

public class HorarioAtencionAppService : IHorarioAtencionAppService
{
    private readonly IUnitOfWork _unitOfWork;

    public HorarioAtencionAppService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<HorarioAtencionDto>> GetByRecursoAsync(int recursoId, CancellationToken cancellationToken = default)
    {
        await ValidarRecursoAsync(recursoId, cancellationToken);

        var horarios = await _unitOfWork.HorariosAtencion.GetByRecursoIdAsync(recursoId, cancellationToken);
        return horarios.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<HorarioAtencionDto>> ReemplazarAsync(int recursoId, ReemplazarHorariosDto dto, CancellationToken cancellationToken = default)
    {
        await ValidarRecursoAsync(recursoId, cancellationToken);

        foreach (var item in dto.Horarios)
        {
            if (item.HoraInicio >= item.HoraFin)
                throw new BadRequestException(
                    $"El horario del {item.DiaSemana} tiene una hora de inicio posterior o igual a la de fin.");
        }

        var existentes = await _unitOfWork.HorariosAtencion.GetByRecursoIdAsync(recursoId, cancellationToken);
        foreach (var existente in existentes)
        {
            _unitOfWork.HorariosAtencion.Delete(existente);
        }

        var nuevos = dto.Horarios.Select(item => new HorarioAtencion
        {
            RecursoId = recursoId,
            DiaSemana = item.DiaSemana,
            HoraInicio = item.HoraInicio,
            HoraFin = item.HoraFin
        }).ToList();

        foreach (var nuevo in nuevos)
        {
            await _unitOfWork.HorariosAtencion.AddAsync(nuevo, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return nuevos.Select(MapToDto).ToList();
    }

    private async Task ValidarRecursoAsync(int recursoId, CancellationToken cancellationToken)
    {
        var recurso = await _unitOfWork.Recursos.GetByIdAsync(recursoId, cancellationToken);
        if (recurso is null)
            throw new NotFoundException(nameof(Recurso), recursoId);
    }

    private static HorarioAtencionDto MapToDto(HorarioAtencion h)
        => new(h.Id, h.DiaSemana, h.HoraInicio, h.HoraFin);
}
