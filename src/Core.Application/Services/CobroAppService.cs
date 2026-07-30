using System;
using System.Collections.Generic;
using System.Text;
using TurnosApp.Core.Application.DTOs.Cobros;
using TurnosApp.Core.Application.Exceptions;
using TurnosApp.Core.Application.Interfaces.Persistence;
using TurnosApp.Core.Application.Interfaces.Services;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Core.Domain.Enums;
using TurnosApp.Core.Exceptions;

namespace TurnosApp.Core.Application.Services;

public class CobroAppService : ICobroAppService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantProvider _tenantProvider;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICajaAppService _cajaAppService;

    public CobroAppService(
        IUnitOfWork unitOfWork,
        ITenantProvider tenantProvider,
        ICurrentUserService currentUserService,
        ICajaAppService cajaAppService)
    {
        _unitOfWork = unitOfWork;
        _tenantProvider = tenantProvider;
        _currentUserService = currentUserService;
        _cajaAppService = cajaAppService;
    }

    public async Task<CobroDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var cobro = await _unitOfWork.Cobros.GetByIdAsync(id, cancellationToken);

        if (cobro is null)
            throw new NotFoundException(nameof(Cobro), id);

        var puedeVerGananciaNeta = await PuedeVerGananciaNetaAsync(cancellationToken);
        return MapToDto(cobro, puedeVerGananciaNeta);
    }

    public async Task<IReadOnlyList<CobroDto>> GetAllByTurnoIdAsync(int turnoId, CancellationToken cancellationToken = default)
    {
        var turno = await _unitOfWork.Turnos.GetByIdAsync(turnoId, cancellationToken);

        if (turno is null)
            throw new NotFoundException(nameof(Turno), turnoId);

        var cobros = await _unitOfWork.Cobros.GetAllByTurnoIdAsync(turnoId, cancellationToken);

        var puedeVerGananciaNeta = await PuedeVerGananciaNetaAsync(cancellationToken);
        return cobros.OrderBy(c => c.CreadoEn).Select(c => MapToDto(c, puedeVerGananciaNeta)).ToList();
    }

    public async Task<CobroDto> CrearCobroAsync(CreateCobroDto dto, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        // Paso 1: Turno con detalles (TurnoServicios + Cobros) para calcular PrecioTotal/SaldoPendiente
        var turno = await _unitOfWork.Turnos.GetByIdConDetallesAsync(dto.TurnoId, cancellationToken);

        if (turno is null)
            throw new NotFoundException(nameof(Turno), dto.TurnoId);

        // Paso 2: Estado del turno
        if (turno.Estado is EstadoTurno.Cancelado or EstadoTurno.Ausente)
            throw new BusinessException(
                code: "TURNO_NO_COBRABLE",
                message: $"El turno {dto.TurnoId} está en estado '{turno.Estado}' y no admite cobros.");

        // Paso 3: MetodoPago vigente
        var metodoPago = await _unitOfWork.MetodoPagos.GetByIdAsync(dto.MetodoPagoId, cancellationToken);

        if (metodoPago is null)
            throw new NotFoundException(nameof(MetodoPago), dto.MetodoPagoId);

        if (!metodoPago.Activo)
            throw new BusinessException(
                code: "METODO_PAGO_INACTIVO",
                message: $"El método de pago '{metodoPago.Nombre}' está desactivado y no puede usarse en nuevos cobros.");

        // Paso 4: Monto
        if (dto.PrecioBase <= 0)
            throw new BusinessException(code: "MONTO_INVALIDO", message: "El monto del cobro debe ser mayor a cero.");

        var precioTotal = turno.TurnoServicios.Sum(ts => ts.PrecioAplicado);
        var montoCobrado = turno.Cobros.Sum(c => c.PrecioBase);
        var saldoPendiente = precioTotal - montoCobrado;

        if (dto.PrecioBase > saldoPendiente)
            throw new BusinessException(
                code: "MONTO_EXCEDE_SALDO",
                message: $"El monto {dto.PrecioBase:F2} supera el saldo pendiente ({saldoPendiente:F2}) del turno {dto.TurnoId}.");

        // Paso 5: Construir + snapshotear
        var cobro = new Cobro
        {
            TenantId = tenantId,
            TurnoId = dto.TurnoId,
            MetodoPagoId = metodoPago.Id,
            NombreMetodoPagoSnapshot = metodoPago.Nombre,
            TipoModificadorSnapshot = metodoPago.TipoModificador,
            PorcentajeModificadorSnapshot = metodoPago.PorcentajeModificador,
            PorcentajeComisionSnapshot = metodoPago.PorcentajeComision,
            PrecioBase = dto.PrecioBase,
            CreadoPor = tenantId.ToString()
        };

        await _unitOfWork.Cobros.AddAsync(cobro, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Genera el movimiento de caja automático si hay una sesión abierta — no-op si no la hay.
        await _cajaAppService.SincronizarMovimientoDeCobroAsync(cobro, cancellationToken);

        var puedeVerGananciaNeta = await PuedeVerGananciaNetaAsync(cancellationToken);
        return MapToDto(cobro, puedeVerGananciaNeta);
    }

    public async Task<CobroDto> ActualizarCobroAsync(int id, UpdateCobroDto dto, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();

        var cobro = await _unitOfWork.Cobros.GetByIdAsync(id, cancellationToken);

        if (cobro is null)
            throw new NotFoundException(nameof(Cobro), id);

        var turno = await _unitOfWork.Turnos.GetByIdConDetallesAsync(cobro.TurnoId, cancellationToken);

        if (turno is null)
            throw new NotFoundException(nameof(Turno), cobro.TurnoId);

        if (turno.Estado is EstadoTurno.Cancelado or EstadoTurno.Ausente)
            throw new BusinessException(
                code: "TURNO_NO_COBRABLE",
                message: $"El turno {cobro.TurnoId} está en estado '{turno.Estado}' y no admite edición de cobros.");

        var metodoPago = await _unitOfWork.MetodoPagos.GetByIdAsync(dto.MetodoPagoId, cancellationToken);

        if (metodoPago is null)
            throw new NotFoundException(nameof(MetodoPago), dto.MetodoPagoId);

        if (!metodoPago.Activo)
            throw new BusinessException(
                code: "METODO_PAGO_INACTIVO",
                message: $"El método de pago '{metodoPago.Nombre}' está desactivado y no puede usarse.");

        if (dto.PrecioBase <= 0)
            throw new BusinessException(code: "MONTO_INVALIDO", message: "El monto del cobro debe ser mayor a cero.");

        // Saldo disponible EXCLUYENDO el monto actual de este mismo cobro (se está reemplazando, no sumando).
        var precioTotal = turno.TurnoServicios.Sum(ts => ts.PrecioAplicado);
        var montoCobradoSinEste = turno.Cobros.Where(c => c.Id != id).Sum(c => c.PrecioBase);
        var saldoDisponible = precioTotal - montoCobradoSinEste;

        if (dto.PrecioBase > saldoDisponible)
            throw new BusinessException(
                code: "MONTO_EXCEDE_SALDO",
                message: $"El monto {dto.PrecioBase:F2} supera el saldo disponible ({saldoDisponible:F2}) del turno {cobro.TurnoId}.");

        // Re-tomar el % VIGENTE del MetodoPago seleccionado — nunca mantener el snapshot viejo.
        cobro.MetodoPagoId = metodoPago.Id;
        cobro.NombreMetodoPagoSnapshot = metodoPago.Nombre;
        cobro.TipoModificadorSnapshot = metodoPago.TipoModificador;
        cobro.PorcentajeModificadorSnapshot = metodoPago.PorcentajeModificador;
        cobro.PorcentajeComisionSnapshot = metodoPago.PorcentajeComision;
        cobro.PrecioBase = dto.PrecioBase;
        cobro.ModificadoPor = tenantId.ToString();

        _unitOfWork.Cobros.Update(cobro);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Reversa + reemplaza el movimiento de caja automático si el cobro editado ya tenía
        // uno en una sesión que sigue abierta — no-op si no hay sesión abierta.
        await _cajaAppService.SincronizarMovimientoDeCobroAsync(cobro, cancellationToken);

        var puedeVerGananciaNeta = await PuedeVerGananciaNetaAsync(cancellationToken);
        return MapToDto(cobro, puedeVerGananciaNeta);
    }

    public async Task<HistorialCobrosDto> GetHistorialAsync(
        DateTime? fechaDesde,
        DateTime? fechaHasta,
        string? busqueda,
        int pagina,
        int tamanoPagina,
        CancellationToken cancellationToken = default)
    {
        pagina = Math.Max(pagina, 1);
        tamanoPagina = Math.Clamp(tamanoPagina, 1, 100);

        var (items, totalCount, sumaPrecioFinal, sumaComision) = await _unitOfWork.Cobros.GetHistorialAsync(
            fechaDesde, fechaHasta, busqueda, pagina, tamanoPagina, cancellationToken);

        // Saldo pendiente global: "cuánto te deben ahora mismo", independiente del filtro de fecha del historial.
        var turnos = await _unitOfWork.Turnos.GetAllConDetallesAsync(cancellationToken);

        var saldoPendienteGlobal = turnos
            .Where(t => t.Estado is not (EstadoTurno.Cancelado or EstadoTurno.Ausente))
            .Sum(t => t.TurnoServicios.Sum(ts => ts.PrecioAplicado) - t.Cobros.Sum(c => c.PrecioBase));

        var puedeVerGananciaNeta = await PuedeVerGananciaNetaAsync(cancellationToken);

        return new HistorialCobrosDto(
            Items: items.Select(c => MapToListItemDto(c, puedeVerGananciaNeta)).ToList(),
            TotalCount: totalCount,
            Pagina: pagina,
            TamanoPagina: tamanoPagina,
            TotalCobradoPeriodo: sumaPrecioFinal,
            ComisionesTotalesPeriodo: puedeVerGananciaNeta ? sumaComision : null,
            SaldoPendienteGlobal: saldoPendienteGlobal
        );
    }

    private async Task<bool> PuedeVerGananciaNetaAsync(CancellationToken cancellationToken)
    {
        var permisos = await _currentUserService.GetCurrentPermisosAsync(cancellationToken);
        return permisos.HasFlag(Permiso.VerGananciaNeta);
    }

    private static CobroListItemDto MapToListItemDto(Cobro c, bool puedeVerGananciaNeta) => new(
        Id: c.Id,
        TurnoId: c.TurnoId,
        ClienteNombreCompleto: $"{c.Turno.Cliente?.Nombre} {c.Turno.Cliente?.Apellido}".Trim(),
        ServiciosResumen: string.Join(", ", c.Turno.TurnoServicios.Select(ts => ts.Servicio?.Nombre ?? string.Empty)),
        FechaHoraTurno: c.Turno.FechaHoraInicio,
        MetodoPagoId: c.MetodoPagoId,
        NombreMetodoPagoSnapshot: c.NombreMetodoPagoSnapshot,
        TipoModificadorSnapshot: c.TipoModificadorSnapshot.ToString(),
        PorcentajeModificadorSnapshot: c.PorcentajeModificadorSnapshot,
        PrecioBase: c.PrecioBase,
        MontoModificadorCliente: c.MontoModificadorCliente,
        PrecioFinal: c.PrecioFinal,
        MontoComision: puedeVerGananciaNeta ? c.MontoComision : null,
        GananciaNeta: puedeVerGananciaNeta ? c.GananciaNeta : null,
        CreadoEn: c.CreadoEn
    );

    private static CobroDto MapToDto(Cobro c, bool puedeVerGananciaNeta) => new(
        Id: c.Id,
        TurnoId: c.TurnoId,
        MetodoPagoId: c.MetodoPagoId,
        NombreMetodoPagoSnapshot: c.NombreMetodoPagoSnapshot,
        TipoModificadorSnapshot: c.TipoModificadorSnapshot.ToString(),
        PorcentajeModificadorSnapshot: c.PorcentajeModificadorSnapshot,
        PorcentajeComisionSnapshot: c.PorcentajeComisionSnapshot,
        PrecioBase: c.PrecioBase,
        MontoModificadorCliente: c.MontoModificadorCliente,
        PrecioFinal: c.PrecioFinal,
        MontoComision: puedeVerGananciaNeta ? c.MontoComision : null,
        GananciaNeta: puedeVerGananciaNeta ? c.GananciaNeta : null,
        CreadoEn: c.CreadoEn,
        CreadoPor: c.CreadoPor,
        ModificadoEn: c.ModificadoEn,
        ModificadoPor: c.ModificadoPor
    );
}
