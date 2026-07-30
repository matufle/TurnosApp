using System;
using System.Collections.Generic;
using System.Text;
using TurnosApp.Core.Application.Common;
using TurnosApp.Core.Application.DTOs;
using TurnosApp.Core.Application.Exceptions;
using TurnosApp.Core.Application.Interfaces;
using TurnosApp.Core.Application.Interfaces.Persistence;
using TurnosApp.Core.Application.Interfaces.Services;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Core.Domain.Enums;
using TurnosApp.Core.Exceptions;

namespace TurnosApp.Core.Application.Services;

public class AuthAppService : IAuthAppService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasherService _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly INotificacionAppService _notificacionAppService;
    private readonly ITurnstileService _turnstileService;

    public AuthAppService(
        IUnitOfWork unitOfWork,
        IPasswordHasherService passwordHasher,
        IJwtTokenService jwtTokenService,
        INotificacionAppService notificacionAppService,
        ITurnstileService turnstileService)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _notificacionAppService = notificacionAppService;
        _turnstileService = turnstileService;
    }

    public async Task<RegistroPendienteDto> RegisterAsync(RegisterRequestDTO dto, CancellationToken cancellationToken)
    {
        if (!await _turnstileService.VerificarAsync(dto.TurnstileToken, cancellationToken))
            throw new BusinessException(code: "CAPTCHA_INVALIDO", message: "No pudimos verificar que sos humano. Intentá de nuevo.");

        var emailExistente = await _unitOfWork.Usuarios.GetByEmailGlobalAsync(dto.Email, cancellationToken);

        if (emailExistente is not null)
            throw new ConflictException("Ya existe una cuenta registrada con ese email.");

        var tenant = new Tenant
        {
            Nombre = dto.NombreNegocio,
            Slug = GenerarSlug(dto.NombreNegocio),
        };

        await _unitOfWork.Tenants.AddAsync(tenant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken); // necesario: EF recién asigna tenant.Id acá

        var rolAdmin = await SeedearRolesAsync(tenant.Id, cancellationToken);
        await SeedearMetodoPagoEfectivoAsync(tenant.Id, cancellationToken);

        var passwordHash = _passwordHasher.HashPassword(dto.Password);
        var nombre = dto.Email.Split('@')[0];
        var usuario = new Usuario(nombre, dto.Email, passwordHash, tenant.Id, rolAdmin.Id);

        var token = SecureTokenGenerator.Generar();
        usuario.EstablecerTokenConfirmacion(token, DateTime.UtcNow.AddHours(48));

        await _unitOfWork.Usuarios.AddAsync(usuario, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _notificacionAppService.ProgramarConfirmacionEmailUsuarioAsync(usuario, token, cancellationToken);

        return new RegistroPendienteDto(usuario.Email);
    }

    // Seedea los 3 roles default de un tenant nuevo. Las entidades Rol traen el TenantId
    // ya asignado a mano: el registro es un flujo anónimo, todavía no hay ITenantProvider
    // que resolver (ver el chequeo "TenantId == 0" en GenericRepository.AddAsync).
    private async Task<Rol> SeedearRolesAsync(int tenantId, CancellationToken cancellationToken)
    {
        var admin = new Rol { TenantId = tenantId, Nombre = "Admin", EsSistema = true, Permisos = Permiso.Todos };
        var empleado = new Rol
        {
            TenantId = tenantId,
            Nombre = "Empleado",
            EsSistema = false,
            Permisos = Permiso.GestionarTurnos | Permiso.CrearCobros | Permiso.GestionarListaEspera
        };
        var recepcionista = new Rol
        {
            TenantId = tenantId,
            Nombre = "Recepcionista",
            EsSistema = false,
            Permisos = Permiso.VerAgendaCompleta | Permiso.GestionarTurnos | Permiso.GestionarClientes | Permiso.CrearCobros
                | Permiso.GestionarListaEspera
        };

        await _unitOfWork.Roles.AddAsync(admin, cancellationToken);
        await _unitOfWork.Roles.AddAsync(empleado, cancellationToken);
        await _unitOfWork.Roles.AddAsync(recepcionista, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken); // necesario: EF recién asigna admin.Id acá

        return admin;
    }

    // Efectivo es el único medio de pago que TODO negocio usa desde el día uno — sin esto,
    // un tenant nuevo arranca sin nada que elegir en "Registrar movimiento" de Caja hasta
    // que alguien va manualmente a Métodos de Pago a crearlo. Igual que los roles, el tenant
    // puede editarlo o desactivarlo después si no lo necesita.
    private async Task SeedearMetodoPagoEfectivoAsync(int tenantId, CancellationToken cancellationToken)
    {
        var efectivo = new MetodoPago
        {
            TenantId = tenantId,
            Nombre = "Efectivo",
            TipoModificador = TipoModificadorPago.Ninguno,
            PorcentajeModificador = 0,
            PorcentajeComision = 0,
            EsEfectivo = true,
            Activo = true
        };

        await _unitOfWork.MetodoPagos.AddAsync(efectivo, cancellationToken);
    }

    private static string GenerarSlug(string nombre)
    {
        return nombre
            .ToLowerInvariant()
            .Trim()
            .Replace(" ", "-")
            .Normalize(System.Text.NormalizationForm.FormD); // simplificado — ver nota abajo
    }

    public async Task<LoginResponseDTO> LoginAsync(LoginRequestDTO dto, CancellationToken cancellationToken)
    {
        var usuario = await _unitOfWork.Usuarios.GetByEmailGlobalAsync(dto.Email, cancellationToken);

        if (usuario is null || !_passwordHasher.VerifyPassword(usuario.PasswordHash, dto.Password))
            throw new BusinessException(code: "CREDENCIALES_INVALIDAS", message: "Email o contraseña incorrectos.");

        if (!usuario.Activo)
            throw new BusinessException(code: "USUARIO_INACTIVO", message: "Tu usuario fue desactivado. Contactá al administrador de tu negocio.");

        if (!usuario.EmailConfirmado)
            throw new BusinessException(code: "EMAIL_NO_CONFIRMADO", message: "Confirmá tu email antes de ingresar. Revisá tu casilla de entrada.");

        var token = _jwtTokenService.GenerateToken(usuario, dto.RecordarMe);

        return new LoginResponseDTO(token, usuario.TenantId, usuario.Email);
    }

    public async Task ConfirmarEmailAsync(string token, CancellationToken cancellationToken)
    {
        var usuario = await _unitOfWork.Usuarios.GetByTokenConfirmacionAsync(token, cancellationToken);

        if (usuario is null)
            throw new BusinessException(code: "TOKEN_INVALIDO", message: "El link de confirmación no es válido.");

        if (usuario.TokenConfirmacionExpira is null || usuario.TokenConfirmacionExpira < DateTime.UtcNow)
            throw new BusinessException(code: "TOKEN_EXPIRADO", message: "El link de confirmación expiró. Solicitá uno nuevo.");

        usuario.ConfirmarEmail();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ReenviarConfirmacionAsync(string email, CancellationToken cancellationToken)
    {
        var usuario = await _unitOfWork.Usuarios.GetByEmailGlobalAsync(email, cancellationToken);

        // Nunca revelamos si el email existe (mismo criterio anti-enumeración que "olvidé mi
        // contraseña"): si no existe o ya está confirmado, no hacemos nada pero igual "éxito".
        if (usuario is null || usuario.EmailConfirmado)
            return;

        var token = SecureTokenGenerator.Generar();
        usuario.EstablecerTokenConfirmacion(token, DateTime.UtcNow.AddHours(48));
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _notificacionAppService.ProgramarConfirmacionEmailUsuarioAsync(usuario, token, cancellationToken);
    }
}
