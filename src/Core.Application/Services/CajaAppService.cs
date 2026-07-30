using TurnosApp.Core.Application.DTOs.Caja;
using TurnosApp.Core.Application.Exceptions;
using TurnosApp.Core.Application.Interfaces.Persistence;
using TurnosApp.Core.Application.Interfaces.Services;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Core.Domain.Enums;
using TurnosApp.Core.Exceptions;

namespace TurnosApp.Core.Application.Services;

public class CajaAppService : ICajaAppService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CajaAppService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<SesionCajaDto?> GetSesionAbiertaAsync(CancellationToken cancellationToken = default)
    {
        var sesion = await _unitOfWork.SesionesCaja.GetAbiertaAsync(cancellationToken);

        if (sesion is null)
            return null;

        var nombres = await ResolverNombresUsuarioAsync(cancellationToken);
        return MapToDto(sesion, nombres);
    }

    public async Task<SesionCajaDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var sesion = await _unitOfWork.SesionesCaja.GetByIdConMovimientosAsync(id, cancellationToken);

        if (sesion is null)
            throw new NotFoundException(nameof(SesionCaja), id);

        var nombres = await ResolverNombresUsuarioAsync(cancellationToken);
        return MapToDto(sesion, nombres);
    }

    public async Task<SesionCajaDto> AbrirSesionAsync(AbrirSesionCajaDto dto, CancellationToken cancellationToken = default)
    {
        var existente = await _unitOfWork.SesionesCaja.GetAbiertaAsync(cancellationToken);

        if (existente is not null)
            throw new BusinessException(
                code: "SESION_CAJA_YA_ABIERTA",
                message: "Ya hay una sesión de caja abierta. Cerrala antes de abrir una nueva.");

        if (dto.MontoInicial < 0)
            throw new BusinessException(code: "MONTO_INVALIDO", message: "El monto inicial no puede ser negativo.");

        var sesion = new SesionCaja
        {
            UsuarioAperturaId = _currentUserService.GetCurrentUsuarioId(),
            FechaApertura = DateTime.UtcNow,
            MontoInicial = dto.MontoInicial,
            Estado = EstadoSesionCaja.Abierta,
            Observaciones = dto.Observaciones
        };

        await _unitOfWork.SesionesCaja.AddAsync(sesion, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var nombres = await ResolverNombresUsuarioAsync(cancellationToken);
        return MapToDto(sesion, nombres);
    }

    public async Task<MovimientoCajaDto> RegistrarMovimientoAsync(RegistrarMovimientoCajaDto dto, CancellationToken cancellationToken = default)
    {
        var sesion = await _unitOfWork.SesionesCaja.GetAbiertaAsync(cancellationToken);

        if (sesion is null)
            throw new BusinessException(
                code: "SESION_CAJA_NO_ABIERTA",
                message: "No hay una sesión de caja abierta para registrar movimientos.");

        var tipo = ParseTipoMovimiento(dto.Tipo);

        if (dto.Monto <= 0)
            throw new BusinessException(code: "MONTO_INVALIDO", message: "El monto del movimiento debe ser mayor a cero.");

        var metodoPago = await _unitOfWork.MetodoPagos.GetByIdAsync(dto.MetodoPagoId, cancellationToken);

        if (metodoPago is null)
            throw new NotFoundException(nameof(MetodoPago), dto.MetodoPagoId);

        if (!metodoPago.Activo)
            throw new BusinessException(
                code: "METODO_PAGO_INACTIVO",
                message: $"El método de pago '{metodoPago.Nombre}' está desactivado y no puede usarse.");

        var movimiento = new MovimientoCaja
        {
            SesionCajaId = sesion.Id,
            Tipo = tipo,
            Monto = dto.Monto,
            MetodoPagoId = metodoPago.Id,
            NombreMetodoPagoSnapshot = metodoPago.Nombre,
            EsEfectivoSnapshot = metodoPago.EsEfectivo,
            Concepto = dto.Concepto,
            FechaHora = DateTime.UtcNow,
            UsuarioId = _currentUserService.GetCurrentUsuarioId()
        };

        await _unitOfWork.MovimientosCaja.AddAsync(movimiento, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var nombres = await ResolverNombresUsuarioAsync(cancellationToken);
        return MapMovimientoToDto(movimiento, nombres);
    }

    public async Task<SesionCajaDto> CerrarSesionAsync(int id, CerrarSesionCajaDto dto, CancellationToken cancellationToken = default)
    {
        var sesion = await _unitOfWork.SesionesCaja.GetByIdConMovimientosAsync(id, cancellationToken);

        if (sesion is null)
            throw new NotFoundException(nameof(SesionCaja), id);

        if (sesion.Estado == EstadoSesionCaja.Cerrada)
            throw new BusinessException(code: "SESION_CAJA_YA_CERRADA", message: "Esta sesión de caja ya está cerrada.");

        if (dto.MontoFinalDeclarado < 0)
            throw new BusinessException(code: "MONTO_INVALIDO", message: "El monto final declarado no puede ser negativo.");

        var usuarioActualId = _currentUserService.GetCurrentUsuarioId();
        var cierreForzado = false;

        // Solo quien abrió la sesión puede cerrarla en el flujo normal — cerrar la de otro
        // requiere el permiso separado ForzarCierreCaja (cubre tanto "se olvidó de cerrar
        // ayer" como cualquier otro caso de sesión ajena).
        if (sesion.UsuarioAperturaId != usuarioActualId)
        {
            var permisos = await _currentUserService.GetCurrentPermisosAsync(cancellationToken);

            if (!permisos.HasFlag(Permiso.ForzarCierreCaja))
                throw new ForbiddenException(
                    "Solo quien abrió la caja puede cerrarla. Para cerrar una sesión abierta por otro usuario necesitás el permiso 'Forzar cierre de caja'.");

            cierreForzado = true;
        }

        sesion.MontoFinalDeclarado = dto.MontoFinalDeclarado;
        sesion.FechaCierre = DateTime.UtcNow;
        sesion.Estado = EstadoSesionCaja.Cerrada;
        sesion.UsuarioCierreId = usuarioActualId;
        sesion.CierreForzado = cierreForzado;

        if (!string.IsNullOrWhiteSpace(dto.Observaciones))
            sesion.Observaciones = dto.Observaciones;

        _unitOfWork.SesionesCaja.Update(sesion);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var nombres = await ResolverNombresUsuarioAsync(cancellationToken);
        return MapToDto(sesion, nombres);
    }

    public async Task<HistorialSesionesCajaDto> GetHistorialAsync(
        DateTime? fechaDesde,
        DateTime? fechaHasta,
        int pagina,
        int tamanoPagina,
        CancellationToken cancellationToken = default)
    {
        pagina = Math.Max(pagina, 1);
        tamanoPagina = Math.Clamp(tamanoPagina, 1, 100);

        var (items, totalCount) = await _unitOfWork.SesionesCaja.GetHistorialAsync(
            fechaDesde, fechaHasta, pagina, tamanoPagina, cancellationToken);

        var nombres = await ResolverNombresUsuarioAsync(cancellationToken);

        return new HistorialSesionesCajaDto(
            Items: items.Select(s => MapToListItemDto(s, nombres)).ToList(),
            TotalCount: totalCount,
            Pagina: pagina,
            TamanoPagina: tamanoPagina
        );
    }

    public async Task SincronizarMovimientoDeCobroAsync(Cobro cobro, CancellationToken cancellationToken = default)
    {
        var sesionAbierta = await _unitOfWork.SesionesCaja.GetAbiertaAsync(cancellationToken);

        // No hay caja abierta: el cobro se registra igual, simplemente no impacta el arqueo.
        if (sesionAbierta is null)
            return;

        var usuarioId = _currentUserService.GetCurrentUsuarioId();

        // Si este Cobro ya había generado un movimiento automático en la sesión abierta
        // (se está editando un cobro ya registrado hoy), lo reversamos antes de crear el
        // nuevo — MovimientoCaja es inmutable, no se pisa el monto/método viejo.
        var movimientoExistente = await _unitOfWork.MovimientosCaja.GetMovimientoActivoDeCobroAsync(cobro.Id, cancellationToken);

        if (movimientoExistente is not null)
        {
            var reversa = new MovimientoCaja
            {
                SesionCajaId = sesionAbierta.Id,
                Tipo = movimientoExistente.Tipo == TipoMovimientoCaja.Ingreso ? TipoMovimientoCaja.Egreso : TipoMovimientoCaja.Ingreso,
                Monto = movimientoExistente.Monto,
                MetodoPagoId = movimientoExistente.MetodoPagoId,
                NombreMetodoPagoSnapshot = movimientoExistente.NombreMetodoPagoSnapshot,
                EsEfectivoSnapshot = movimientoExistente.EsEfectivoSnapshot,
                Concepto = $"Reversa por edición del cobro del turno #{cobro.TurnoId}",
                FechaHora = DateTime.UtcNow,
                UsuarioId = usuarioId,
                CobroId = cobro.Id,
                MovimientoOrigenId = movimientoExistente.Id
            };

            await _unitOfWork.MovimientosCaja.AddAsync(reversa, cancellationToken);
        }

        // El método de pago vigente del cobro ya fue validado (Activo) por CobroAppService
        // antes de llegar acá — lo re-leemos solo para tomar el EsEfectivo actual.
        var metodoPago = cobro.MetodoPagoId is int metodoPagoId
            ? await _unitOfWork.MetodoPagos.GetByIdAsync(metodoPagoId, cancellationToken)
            : null;

        var nuevo = new MovimientoCaja
        {
            SesionCajaId = sesionAbierta.Id,
            Tipo = TipoMovimientoCaja.Ingreso,
            // PrecioFinal (no PrecioBase): es la plata que efectivamente recibe el cajero,
            // ya con el recargo/bonificación del medio de pago aplicado.
            Monto = cobro.PrecioFinal,
            MetodoPagoId = cobro.MetodoPagoId,
            NombreMetodoPagoSnapshot = cobro.NombreMetodoPagoSnapshot,
            EsEfectivoSnapshot = metodoPago?.EsEfectivo ?? false,
            Concepto = $"Cobro turno #{cobro.TurnoId}",
            FechaHora = DateTime.UtcNow,
            UsuarioId = usuarioId,
            CobroId = cobro.Id
        };

        await _unitOfWork.MovimientosCaja.AddAsync(nuevo, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // Los movimientos/sesiones solo guardan UsuarioId — se resuelve a Nombre acá (una sola
    // consulta a todos los usuarios del tenant) en vez de N+1 por movimiento.
    private async Task<Dictionary<int, string>> ResolverNombresUsuarioAsync(CancellationToken cancellationToken)
    {
        var usuarios = await _unitOfWork.Usuarios.GetAllAsync(cancellationToken);
        return usuarios.ToDictionary(u => u.Id, u => u.Nombre);
    }

    private static string ResolverNombre(Dictionary<int, string> nombres, int usuarioId) =>
        nombres.TryGetValue(usuarioId, out var nombre) ? nombre : $"Usuario #{usuarioId}";

    private static TipoMovimientoCaja ParseTipoMovimiento(string valor)
    {
        if (!Enum.TryParse<TipoMovimientoCaja>(valor, ignoreCase: true, out var tipo))
            throw new BadRequestException($"'{valor}' no es un tipo de movimiento de caja válido.");

        return tipo;
    }

    private static SesionCajaDto MapToDto(SesionCaja s, Dictionary<int, string> nombres)
    {
        var movimientos = s.Movimientos.OrderBy(m => m.FechaHora).Select(m => MapMovimientoToDto(m, nombres)).ToList();

        var desglose = s.Movimientos
            .GroupBy(m => (m.MetodoPagoId, m.NombreMetodoPagoSnapshot, m.EsEfectivoSnapshot))
            .Select(g => new DesgloseMedioPagoDto(
                MetodoPagoId: g.Key.MetodoPagoId,
                Nombre: g.Key.NombreMetodoPagoSnapshot,
                EsEfectivo: g.Key.EsEfectivoSnapshot,
                TotalIngresos: g.Where(m => m.Tipo == TipoMovimientoCaja.Ingreso).Sum(m => m.Monto),
                TotalEgresos: g.Where(m => m.Tipo == TipoMovimientoCaja.Egreso).Sum(m => m.Monto),
                Total: g.Sum(m => m.Tipo == TipoMovimientoCaja.Ingreso ? m.Monto : -m.Monto)
            ))
            .OrderByDescending(d => d.EsEfectivo)
            .ThenBy(d => d.Nombre)
            .ToList();

        return new SesionCajaDto(
            Id: s.Id,
            FechaApertura: s.FechaApertura,
            FechaCierre: s.FechaCierre,
            MontoInicial: s.MontoInicial,
            MontoFinalDeclarado: s.MontoFinalDeclarado,
            MontoEsperadoEfectivo: s.MontoEsperadoEfectivo,
            Diferencia: s.Diferencia,
            Estado: s.Estado.ToString(),
            CierreForzado: s.CierreForzado,
            Observaciones: s.Observaciones,
            UsuarioAperturaId: s.UsuarioAperturaId,
            UsuarioAperturaNombre: ResolverNombre(nombres, s.UsuarioAperturaId),
            UsuarioCierreId: s.UsuarioCierreId,
            UsuarioCierreNombre: s.UsuarioCierreId is int usuarioCierreId ? ResolverNombre(nombres, usuarioCierreId) : null,
            Movimientos: movimientos,
            DesglosePorMedioPago: desglose
        );
    }

    private static MovimientoCajaDto MapMovimientoToDto(MovimientoCaja m, Dictionary<int, string> nombres) => new(
        Id: m.Id,
        Tipo: m.Tipo.ToString(),
        Monto: m.Monto,
        MetodoPagoId: m.MetodoPagoId,
        NombreMetodoPagoSnapshot: m.NombreMetodoPagoSnapshot,
        EsEfectivoSnapshot: m.EsEfectivoSnapshot,
        Concepto: m.Concepto,
        FechaHora: m.FechaHora,
        UsuarioId: m.UsuarioId,
        UsuarioNombre: ResolverNombre(nombres, m.UsuarioId),
        CobroId: m.CobroId,
        MovimientoOrigenId: m.MovimientoOrigenId
    );

    private static SesionCajaListItemDto MapToListItemDto(SesionCaja s, Dictionary<int, string> nombres) => new(
        Id: s.Id,
        FechaApertura: s.FechaApertura,
        FechaCierre: s.FechaCierre,
        MontoInicial: s.MontoInicial,
        MontoFinalDeclarado: s.MontoFinalDeclarado,
        MontoEsperadoEfectivo: s.MontoEsperadoEfectivo,
        Diferencia: s.Diferencia,
        CierreForzado: s.CierreForzado,
        UsuarioAperturaId: s.UsuarioAperturaId,
        UsuarioAperturaNombre: ResolverNombre(nombres, s.UsuarioAperturaId),
        UsuarioCierreId: s.UsuarioCierreId,
        UsuarioCierreNombre: s.UsuarioCierreId is int usuarioCierreId ? ResolverNombre(nombres, usuarioCierreId) : null
    );
}
