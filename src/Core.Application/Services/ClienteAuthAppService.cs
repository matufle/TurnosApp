using TurnosApp.Core.Application.DTOs.ClienteAuth;
using TurnosApp.Core.Application.Exceptions;
using TurnosApp.Core.Application.Interfaces.Persistence;
using TurnosApp.Core.Application.Interfaces.Services;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Core.Exceptions;

namespace TurnosApp.Core.Application.Services;

public class ClienteAuthAppService : IClienteAuthAppService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasherService _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPublicAppService _publicAppService;
    private readonly ICurrentClienteService _currentClienteService;

    public ClienteAuthAppService(
        IUnitOfWork unitOfWork,
        IPasswordHasherService passwordHasher,
        IJwtTokenService jwtTokenService,
        IPublicAppService publicAppService,
        ICurrentClienteService currentClienteService)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _publicAppService = publicAppService;
        _currentClienteService = currentClienteService;
    }

    public async Task<ClienteAuthResponseDto> RegistrarAsync(ClienteRegistroDto dto, CancellationToken cancellationToken = default)
    {
        var tenant = await _publicAppService.ResolverTenantPorSlugAsync(dto.TenantSlug, cancellationToken);
        var emailNormalizado = dto.Email.Trim().ToLowerInvariant();

        var existente = await _unitOfWork.Clientes.GetByTenantYEmailAsync(tenant.TenantId, emailNormalizado, cancellationToken);
        var passwordHash = _passwordHasher.HashPassword(dto.Password);

        Cliente cliente;

        if (existente is not null)
        {
            if (existente.PasswordHash is not null)
                throw new ConflictException("Ya existe una cuenta registrada con ese email.");

            // Reclama la fila cargada antes por el staff (walk-in): el cliente pasa a ser
            // dueño de sus propios datos, y conserva su historial de turnos previos.
            existente.Nombre = dto.Nombre;
            existente.Apellido = dto.Apellido;
            existente.Email = emailNormalizado;
            existente.Telefono = dto.Telefono;
            existente.PasswordHash = passwordHash;

            _unitOfWork.Clientes.Update(existente);
            cliente = existente;
        }
        else
        {
            cliente = new Cliente
            {
                TenantId = tenant.TenantId, // asignado a mano: flujo anónimo, ITenantProvider no resuelve nada acá
                Nombre = dto.Nombre,
                Apellido = dto.Apellido,
                Email = emailNormalizado,
                Telefono = dto.Telefono,
                PasswordHash = passwordHash,
                Activo = true
            };

            await _unitOfWork.Clientes.AddAsync(cliente, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var token = _jwtTokenService.GenerateToken(cliente);

        return new ClienteAuthResponseDto(token, cliente.TenantId, cliente.Email!);
    }

    public async Task<ClienteAuthResponseDto> LoginAsync(ClienteLoginDto dto, CancellationToken cancellationToken = default)
    {
        var tenant = await _publicAppService.ResolverTenantPorSlugAsync(dto.TenantSlug, cancellationToken);
        var emailNormalizado = dto.Email.Trim().ToLowerInvariant();

        var cliente = await _unitOfWork.Clientes.GetByTenantYEmailAsync(tenant.TenantId, emailNormalizado, cancellationToken);

        if (cliente is null || cliente.PasswordHash is null || !_passwordHasher.VerifyPassword(cliente.PasswordHash, dto.Password))
            throw new BusinessException(code: "CREDENCIALES_INVALIDAS", message: "Email o contraseña incorrectos.");

        if (!cliente.Activo)
            throw new BusinessException(code: "CLIENTE_INACTIVO", message: "Tu cuenta fue desactivada. Contactá al negocio.");

        var token = _jwtTokenService.GenerateToken(cliente, dto.RecordarMe);

        return new ClienteAuthResponseDto(token, cliente.TenantId, cliente.Email!);
    }

    public async Task<ClienteMeDto> GetMeAsync(CancellationToken cancellationToken = default)
    {
        var clienteId = _currentClienteService.GetCurrentClienteId();

        var cliente = await _unitOfWork.Clientes.GetByIdAsync(clienteId, cancellationToken)
            ?? throw new UnauthorizedAccessException("El cliente autenticado ya no existe.");

        return new ClienteMeDto(cliente.Id, cliente.Nombre, cliente.Apellido, cliente.Email ?? string.Empty, cliente.TenantId);
    }
}
