using TurnosApp.Core.Application.DTOs.Public;
using TurnosApp.Core.Application.Interfaces.Persistence;
using TurnosApp.Core.Application.Interfaces.Services;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Core.Exceptions;

namespace TurnosApp.Core.Application.Services;

public class PublicAppService : IPublicAppService
{
    private readonly IUnitOfWork _unitOfWork;

    public PublicAppService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<TenantPublicoDto> ResolverTenantPorSlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var tenant = await _unitOfWork.Tenants.GetBySlugAsync(slug, cancellationToken);

        if (tenant is null || !tenant.Activo || !tenant.PermiteReservasPublicas)
            throw new NotFoundException(nameof(Tenant), slug);

        return new TenantPublicoDto(
            TenantId: tenant.Id,
            Nombre: tenant.Nombre,
            Slug: tenant.Slug,
            ColorPrimario: tenant.ColorPrimario
        );
    }
}
