using TurnosApp.Core.Application.DTOs.AdelantosProfesional;
using TurnosApp.Core.Application.Exceptions;
using TurnosApp.Core.Application.Interfaces.Persistence;
using TurnosApp.Core.Application.Interfaces.Services;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Core.Exceptions;

namespace TurnosApp.Core.Application.Services;

public class AdelantoProfesionalAppService : IAdelantoProfesionalAppService
{
    private readonly IUnitOfWork _unitOfWork;

    public AdelantoProfesionalAppService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<AdelantoProfesionalDto>> GetByRecursoAsync(int recursoId, CancellationToken cancellationToken = default)
    {
        var adelantos = await _unitOfWork.AdelantosProfesional.GetByRecursoAsync(recursoId, cancellationToken);
        return adelantos.Select(MapToDto).ToList();
    }

    public async Task<AdelantoProfesionalDto> CreateAsync(CreateAdelantoProfesionalDto dto, CancellationToken cancellationToken = default)
    {
        var recurso = await _unitOfWork.Recursos.GetByIdAsync(dto.RecursoId, cancellationToken);
        if (recurso is null)
            throw new NotFoundException(nameof(Recurso), dto.RecursoId);

        if (dto.Monto <= 0)
            throw new BusinessException(code: "MONTO_INVALIDO", message: "El monto del adelanto debe ser mayor a cero.");

        var adelanto = new AdelantoProfesional
        {
            RecursoId = dto.RecursoId,
            Monto = dto.Monto,
            Fecha = dto.Fecha,
            Concepto = dto.Concepto
        };

        // Si ya hay una liquidación Generada (no Pagada) cuyo período cubre esta fecha, se
        // asigna al vuelo — si no, queda sin asignar y lo recoge el worker en el próximo ciclo.
        var liquidacionQueCubre = await _unitOfWork.Liquidaciones.GetGeneradaQueCubreAsync(dto.RecursoId, dto.Fecha, cancellationToken);
        if (liquidacionQueCubre is not null)
            adelanto.LiquidacionId = liquidacionQueCubre.Id;

        await _unitOfWork.AdelantosProfesional.AddAsync(adelanto, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(adelanto);
    }

    private static AdelantoProfesionalDto MapToDto(AdelantoProfesional a) => new(
        Id: a.Id,
        RecursoId: a.RecursoId,
        Monto: a.Monto,
        Fecha: a.Fecha,
        Concepto: a.Concepto,
        LiquidacionId: a.LiquidacionId
    );
}
