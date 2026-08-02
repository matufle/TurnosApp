using Microsoft.Extensions.Configuration;
using TurnosApp.Core.Application.Common;
using TurnosApp.Core.Application.DTOs.Suscripcion;
using TurnosApp.Core.Application.Exceptions;
using TurnosApp.Core.Application.Interfaces.Persistence;
using TurnosApp.Core.Application.Interfaces.Services;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Core.Domain.Enums;
using TurnosApp.Core.Exceptions;

namespace TurnosApp.Core.Application.Services;

public class SuscripcionAppService : ISuscripcionAppService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMercadoPagoService _mercadoPagoService;
    private readonly IConfiguration _configuration;

    public SuscripcionAppService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IMercadoPagoService mercadoPagoService,
        IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _mercadoPagoService = mercadoPagoService;
        _configuration = configuration;
    }

    public async Task<SuscripcionDto> GetEstadoAsync(CancellationToken cancellationToken = default)
    {
        var tenant = await ObtenerTenantActualAsync(cancellationToken);

        Plan? plan = tenant.PlanId.HasValue
            ? await _unitOfWork.Planes.GetByIdAsync(tenant.PlanId.Value, cancellationToken)
            : null;

        int? diasRestantesGracia = null;
        if (tenant.EstadoSuscripcion == EstadoSuscripcion.PastDue && tenant.PastDueDesde.HasValue)
        {
            var transcurridos = (DateTime.UtcNow - tenant.PastDueDesde.Value).TotalDays;
            diasRestantesGracia = Math.Max(0, SuscripcionConstantes.DiasGraciaPastDue - (int)Math.Floor(transcurridos));
        }

        return new SuscripcionDto(
            EstadoSuscripcion: tenant.EstadoSuscripcion.ToString(),
            SuscripcionVenceEn: tenant.SuscripcionVenceEn,
            EsGrandfathered: tenant.EsGrandfathered,
            PlanNombre: plan?.Nombre,
            PlanPrecioMensual: plan?.PrecioMensual,
            TieneSuscripcionIniciada: !string.IsNullOrEmpty(tenant.MercadoPagoPreapprovalId),
            DiasRestantesGracia: diasRestantesGracia
        );
    }

    public async Task<string> IniciarSuscripcionAsync(CancellationToken cancellationToken = default)
    {
        var tenant = await ObtenerTenantActualAsync(cancellationToken);

        if (tenant.EstadoSuscripcion == EstadoSuscripcion.Activa)
            throw new BusinessException("SUSCRIPCION_YA_ACTIVA", "Ya tenés una suscripción activa.");

        var usuarioId = _currentUserService.GetCurrentUsuarioId();
        var usuario = await _unitOfWork.Usuarios.GetByIdAsync(usuarioId, cancellationToken)
            ?? throw new NotFoundException(nameof(Usuario), usuarioId);

        var plan = await _unitOfWork.Planes.GetActivoAsync(cancellationToken)
            ?? throw new BusinessException("SIN_PLAN_ACTIVO", "No hay un plan de suscripción configurado todavía.");

        var frontendBaseUrl = _configuration["Frontend:BaseUrl"] ?? "http://localhost:5173";
        var backUrl = $"{frontendBaseUrl}/app/suscripcion";

        // A diferencia de Stripe, MP no requiere crear un "Customer" antes — el preapproval
        // se crea directo con el email del pagador.
        var preapproval = await _mercadoPagoService.CrearPreapprovalAsync(
            usuario.Email,
            $"Suscripción Slotia - {tenant.Nombre}",
            plan.PrecioMensual,
            backUrl,
            cancellationToken);

        tenant.MercadoPagoPreapprovalId = preapproval.Id;
        _unitOfWork.Tenants.Update(tenant);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return preapproval.InitPoint;
    }

    public async Task CancelarSuscripcionAsync(CancellationToken cancellationToken = default)
    {
        var tenant = await ObtenerTenantActualAsync(cancellationToken);

        if (string.IsNullOrEmpty(tenant.MercadoPagoPreapprovalId))
            throw new BusinessException("SIN_SUSCRIPCION", "Todavía no iniciaste ninguna suscripción.");

        await _mercadoPagoService.CancelarPreapprovalAsync(tenant.MercadoPagoPreapprovalId, cancellationToken);

        tenant.EstadoSuscripcion = EstadoSuscripcion.Cancelada;
        tenant.PastDueDesde = null; // Cancelada es un corte inmediato, no hereda gracia de un PastDue previo
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ActualizarDesdeNotificacionAsync(string preapprovalId, CancellationToken cancellationToken = default)
    {
        var tenant = await _unitOfWork.Tenants.GetByMercadoPagoPreapprovalIdAsync(preapprovalId, cancellationToken);
        if (tenant is null)
            return; // notificación de un preapproval que no corresponde a ningún tenant de este sistema

        // El webhook solo trae el id — hay que ir a buscar el estado real a la API, MP no
        // manda el estado en el payload de la notificación.
        var status = await _mercadoPagoService.ObtenerEstadoAsync(preapprovalId, cancellationToken);

        switch (status)
        {
            case "authorized":
                tenant.EstadoSuscripcion = EstadoSuscripcion.Activa;
                tenant.PastDueDesde = null; // se reactivó: se cancela cualquier cuenta de gracia en curso
                break;
            case "paused":
                tenant.EstadoSuscripcion = EstadoSuscripcion.PastDue;
                // Solo la primera vez que entra en este episodio de PastDue — si el webhook
                // vuelve a notificar "paused" en ciclos siguientes, no hay que correr el reloj.
                tenant.PastDueDesde ??= DateTime.UtcNow;
                break;
            case "cancelled":
                tenant.EstadoSuscripcion = EstadoSuscripcion.Cancelada;
                tenant.PastDueDesde = null; // Cancelada es un corte inmediato, sin gracia
                break;
            default:
                return; // "pending" u otro estado transitorio: no tocamos el estado local todavía
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Tenant> ObtenerTenantActualAsync(CancellationToken cancellationToken)
    {
        var tenantId = _currentUserService.GetCurrentTenantId();
        return await _unitOfWork.Tenants.GetByIdAsync(tenantId, cancellationToken)
            ?? throw new NotFoundException(nameof(Tenant), tenantId);
    }
}
