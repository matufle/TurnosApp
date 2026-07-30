using TurnosApp.Core.Application.DTOs.Public;
using TurnosApp.Core.Application.DTOs.Servicios;
using TurnosApp.Core.Application.Exceptions;
using TurnosApp.Core.Application.Interfaces.Persistence;
using TurnosApp.Core.Application.Interfaces.Services;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Core.Exceptions;

namespace TurnosApp.Core.Application.Services;

public class PublicCatalogoAppService : IPublicCatalogoAppService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublicAppService _publicAppService;
    private readonly IDisponibilidadAppService _disponibilidadAppService;

    public PublicCatalogoAppService(
        IUnitOfWork unitOfWork,
        IPublicAppService publicAppService,
        IDisponibilidadAppService disponibilidadAppService)
    {
        _unitOfWork = unitOfWork;
        _publicAppService = publicAppService;
        _disponibilidadAppService = disponibilidadAppService;
    }

    public async Task<IReadOnlyList<ServicioDto>> GetServiciosAsync(string tenantSlug, CancellationToken cancellationToken = default)
    {
        var tenant = await _publicAppService.ResolverTenantPorSlugAsync(tenantSlug, cancellationToken);

        var servicios = await _unitOfWork.Servicios.GetActivosCrossTenantAsync(tenant.TenantId, cancellationToken);

        return servicios.Select(MapServicioToDto).ToList();
    }

    public async Task<IReadOnlyList<RecursoPublicoDto>> GetRecursosAsync(string tenantSlug, CancellationToken cancellationToken = default)
    {
        var tenant = await _publicAppService.ResolverTenantPorSlugAsync(tenantSlug, cancellationToken);

        var recursos = await _unitOfWork.Recursos.GetActivosCrossTenantAsync(tenant.TenantId, cancellationToken);

        return recursos
            .Select(r => new RecursoPublicoDto(r.Id, r.Nombre, r.Descripcion, r.ColorHex))
            .ToList();
    }

    public async Task<IReadOnlyList<string>> GetDisponibilidadAsync(
        string tenantSlug, int recursoId, IReadOnlyList<int> servicioIds, DateOnly fecha, CancellationToken cancellationToken = default)
    {
        var tenant = await _publicAppService.ResolverTenantPorSlugAsync(tenantSlug, cancellationToken);

        if (servicioIds.Count == 0)
            throw new BadRequestException("Tenés que indicar al menos un servicio.");

        // La duración total (suma de todos los servicios elegidos) es la que define el
        // paso de la grilla de horarios — mismo motor que ya usa el flujo de staff.
        var duracionTotalMinutos = 0;
        foreach (var servicioId in servicioIds)
        {
            var servicio = await _unitOfWork.Servicios.GetByIdCrossTenantAsync(tenant.TenantId, servicioId, cancellationToken);
            if (servicio is null || !servicio.Activo)
                throw new NotFoundException(nameof(Servicio), servicioId);

            duracionTotalMinutos += servicio.DuracionMinutos;
        }

        var slots = await _disponibilidadAppService.GetSlotsDisponiblesAsync(
            tenant.TenantId, recursoId, duracionTotalMinutos, fecha, cancellationToken);

        return slots.Select(s => s.ToString("HH:mm")).ToList();
    }

    private static ServicioDto MapServicioToDto(Servicio s)
        => new(s.Id, s.Nombre, s.Descripcion, s.DuracionMinutos, s.Precio, s.Activo);
}
