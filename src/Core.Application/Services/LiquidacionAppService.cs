using TurnosApp.Core.Application.DTOs.AdelantosProfesional;
using TurnosApp.Core.Application.DTOs.Liquidaciones;
using TurnosApp.Core.Application.Exceptions;
using TurnosApp.Core.Application.Interfaces.Persistence;
using TurnosApp.Core.Application.Interfaces.Services;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Core.Domain.Enums;
using TurnosApp.Core.Exceptions;

namespace TurnosApp.Core.Application.Services;

public class LiquidacionAppService : ILiquidacionAppService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public LiquidacionAppService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<LiquidacionListItemDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var liquidaciones = await _unitOfWork.Liquidaciones.GetAllConDetallesAsync(cancellationToken);
        return liquidaciones.Select(MapToListItemDto).ToList();
    }

    public async Task<LiquidacionDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var liquidacion = await _unitOfWork.Liquidaciones.GetByIdConDetallesAsync(id, cancellationToken);
        if (liquidacion is null)
            throw new NotFoundException(nameof(Liquidacion), id);

        return await MapToDtoAsync(liquidacion, cancellationToken);
    }

    public async Task<IReadOnlyList<LiquidacionListItemDto>> GetByRecursoAsync(int recursoId, CancellationToken cancellationToken = default)
    {
        var liquidaciones = await _unitOfWork.Liquidaciones.GetByRecursoConDetallesAsync(recursoId, cancellationToken);
        return liquidaciones.Select(MapToListItemDto).ToList();
    }

    public async Task<LiquidacionDto> MarcarPagadaAsync(int id, MarcarPagadaLiquidacionDto dto, CancellationToken cancellationToken = default)
    {
        var liquidacion = await _unitOfWork.Liquidaciones.GetByIdConDetallesAsync(id, cancellationToken);
        if (liquidacion is null)
            throw new NotFoundException(nameof(Liquidacion), id);

        if (liquidacion.Estado != EstadoLiquidacion.Generada)
            throw new BusinessException(
                code: "LIQUIDACION_NO_GENERADA",
                message: "Solo una liquidación en estado Generada puede marcarse como pagada.");

        liquidacion.Estado = EstadoLiquidacion.Pagada;
        liquidacion.FechaPago = DateTime.UtcNow;
        liquidacion.UsuarioPagoId = _currentUserService.GetCurrentUsuarioId();

        if (!string.IsNullOrWhiteSpace(dto.Observaciones))
            liquidacion.Observaciones = dto.Observaciones;

        _unitOfWork.Liquidaciones.Update(liquidacion);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await MapToDtoAsync(liquidacion, cancellationToken);
    }

    public async Task<LiquidacionDto> AnularAsync(int id, AnularLiquidacionDto dto, CancellationToken cancellationToken = default)
    {
        var liquidacion = await _unitOfWork.Liquidaciones.GetByIdConDetallesAsync(id, cancellationToken);
        if (liquidacion is null)
            throw new NotFoundException(nameof(Liquidacion), id);

        if (liquidacion.Estado == EstadoLiquidacion.Pagada)
            throw new BusinessException(code: "LIQUIDACION_YA_PAGADA", message: "Una liquidación ya pagada no puede anularse.");

        if (liquidacion.Estado == EstadoLiquidacion.Anulada)
            throw new BusinessException(code: "LIQUIDACION_YA_ANULADA", message: "Esta liquidación ya está anulada.");

        liquidacion.Estado = EstadoLiquidacion.Anulada;

        if (!string.IsNullOrWhiteSpace(dto.Observaciones))
            liquidacion.Observaciones = dto.Observaciones;

        // Libera los adelantos de esta liquidación (vuelven a LiquidacionId = null) para que
        // se reconsideren en el próximo ciclo — los detalles NO se borran (historial financiero).
        foreach (var adelanto in liquidacion.Adelantos)
        {
            adelanto.LiquidacionId = null;
            _unitOfWork.AdelantosProfesional.Update(adelanto);
        }

        _unitOfWork.Liquidaciones.Update(liquidacion);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await MapToDtoAsync(liquidacion, cancellationToken);
    }

    private async Task<LiquidacionDto> MapToDtoAsync(Liquidacion l, CancellationToken cancellationToken)
    {
        string? usuarioPagoNombre = null;
        if (l.UsuarioPagoId is int usuarioPagoId)
        {
            var usuario = await _unitOfWork.Usuarios.GetByIdAsync(usuarioPagoId, cancellationToken);
            usuarioPagoNombre = usuario?.Nombre;
        }

        return new LiquidacionDto(
            Id: l.Id,
            RecursoId: l.RecursoId,
            RecursoNombre: l.Recurso.Nombre,
            PeriodoDesde: l.PeriodoDesde,
            PeriodoHasta: l.PeriodoHasta,
            FechaGeneracion: l.FechaGeneracion,
            Estado: l.Estado.ToString(),
            FechaPago: l.FechaPago,
            UsuarioPagoId: l.UsuarioPagoId,
            UsuarioPagoNombre: usuarioPagoNombre,
            Observaciones: l.Observaciones,
            MontoBrutoComision: l.MontoBrutoComision,
            MontoAdelantos: l.MontoAdelantos,
            MontoNeto: l.MontoNeto,
            Detalles: l.Detalles.Select(MapDetalleToDto).ToList(),
            Adelantos: l.Adelantos.Select(MapAdelantoToDto).ToList()
        );
    }

    private static LiquidacionListItemDto MapToListItemDto(Liquidacion l) => new(
        Id: l.Id,
        RecursoId: l.RecursoId,
        RecursoNombre: l.Recurso.Nombre,
        PeriodoDesde: l.PeriodoDesde,
        PeriodoHasta: l.PeriodoHasta,
        FechaGeneracion: l.FechaGeneracion,
        Estado: l.Estado.ToString(),
        MontoBrutoComision: l.MontoBrutoComision,
        MontoAdelantos: l.MontoAdelantos,
        MontoNeto: l.MontoNeto
    );

    private static LiquidacionDetalleDto MapDetalleToDto(LiquidacionDetalle d) => new(
        Id: d.Id,
        TurnoId: d.TurnoId,
        TurnoFecha: d.Turno.FechaHoraInicio,
        ServicioId: d.ServicioId,
        ServicioNombre: d.Servicio.Nombre,
        PrecioBaseAplicado: d.PrecioBaseAplicado,
        TipoComisionSnapshot: d.TipoComisionSnapshot.ToString(),
        ValorComisionSnapshot: d.ValorComisionSnapshot,
        MontoComisionCalculado: d.MontoComisionCalculado
    );

    private static AdelantoProfesionalDto MapAdelantoToDto(AdelantoProfesional a) => new(
        Id: a.Id,
        RecursoId: a.RecursoId,
        Monto: a.Monto,
        Fecha: a.Fecha,
        Concepto: a.Concepto,
        LiquidacionId: a.LiquidacionId
    );
}
