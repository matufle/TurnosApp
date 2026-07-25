using System;
using System.Collections.Generic;
using System.Text;
using TurnosApp.Core.Application.Common;
using TurnosApp.Core.Application.DTOs.Auth;
using TurnosApp.Core.Application.DTOs.Usuarios;
using TurnosApp.Core.Application.Exceptions;
using TurnosApp.Core.Application.Interfaces.Persistence;
using TurnosApp.Core.Application.Interfaces.Services;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Core.Exceptions;

namespace TurnosApp.Core.Application.Services;

public class UsuarioAppService : IUsuarioAppService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasherService _passwordHasher;
    private readonly ICurrentUserService _currentUserService;

    public UsuarioAppService(
        IUnitOfWork unitOfWork,
        IPasswordHasherService passwordHasher,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _currentUserService = currentUserService;
    }

    public async Task<UsuarioDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var usuario = await _unitOfWork.Usuarios.GetByIdAsync(id, cancellationToken);

        if (usuario is null)
            throw new NotFoundException(nameof(Usuario), id);

        return MapToDto(usuario);
    }

    public async Task<IReadOnlyList<UsuarioDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var usuarios = await _unitOfWork.Usuarios.GetAllAsync(cancellationToken);

        return usuarios.Select(MapToDto).ToList();
    }

    public async Task<UsuarioDto> CreateAsync(CreateUsuarioDto dto, CancellationToken cancellationToken = default)
    {
        var emailExistente = await _unitOfWork.Usuarios.GetByEmailGlobalAsync(dto.Email, cancellationToken);
        if (emailExistente is not null)
            throw new ConflictException("Ya existe una cuenta registrada con ese email.");

        var rol = await _unitOfWork.Roles.GetByIdAsync(dto.RolId, cancellationToken);
        if (rol is null)
            throw new NotFoundException(nameof(Rol), dto.RolId);

        var tenantId = _currentUserService.GetCurrentTenantId();
        var passwordHash = _passwordHasher.HashPassword(dto.Password);

        var usuario = new Usuario(dto.Nombre, dto.Email, passwordHash, tenantId, dto.RolId);

        await _unitOfWork.Usuarios.AddAsync(usuario, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Recién guardado: recargamos con Rol incluido para el mapeo.
        var creado = await _unitOfWork.Usuarios.GetByIdAsync(usuario.Id, cancellationToken);
        return MapToDto(creado!);
    }

    public async Task<UsuarioDto> UpdateAsync(int id, UpdateUsuarioDto dto, CancellationToken cancellationToken = default)
    {
        var usuario = await _unitOfWork.Usuarios.GetByIdAsync(id, cancellationToken);
        if (usuario is null)
            throw new NotFoundException(nameof(Usuario), id);

        var rol = await _unitOfWork.Roles.GetByIdAsync(dto.RolId, cancellationToken);
        if (rol is null)
            throw new NotFoundException(nameof(Rol), dto.RolId);

        usuario.ActualizarNombre(dto.Nombre);
        usuario.AsignarRol(dto.RolId);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var actualizado = await _unitOfWork.Usuarios.GetByIdAsync(id, cancellationToken);
        return MapToDto(actualizado!);
    }

    public async Task<UsuarioDto> ActivarAsync(int id, CancellationToken cancellationToken = default)
    {
        var usuario = await _unitOfWork.Usuarios.GetByIdAsync(id, cancellationToken);
        if (usuario is null)
            throw new NotFoundException(nameof(Usuario), id);

        usuario.Activar();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(usuario);
    }

    public async Task<UsuarioDto> DesactivarAsync(int id, CancellationToken cancellationToken = default)
    {
        if (id == _currentUserService.GetCurrentUsuarioId())
            throw new BusinessException("NO_PUEDE_DESACTIVARSE_A_SI_MISMO", "No podés desactivar tu propio usuario.");

        var usuario = await _unitOfWork.Usuarios.GetByIdAsync(id, cancellationToken);
        if (usuario is null)
            throw new NotFoundException(nameof(Usuario), id);

        usuario.Desactivar();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(usuario);
    }

    public async Task<MeDto> GetMeAsync(CancellationToken cancellationToken = default)
    {
        var usuarioId = _currentUserService.GetCurrentUsuarioId();

        var usuario = await _unitOfWork.Usuarios.GetByIdAsync(usuarioId, cancellationToken)
            ?? throw new NotFoundException(nameof(Usuario), usuarioId);

        var recurso = await _unitOfWork.Recursos.GetByUsuarioIdAsync(usuarioId, cancellationToken);

        return new MeDto(
            UsuarioId: usuario.Id,
            Nombre: usuario.Nombre,
            Email: usuario.Email,
            TenantId: usuario.TenantId,
            RolId: usuario.RolId,
            RolNombre: usuario.Rol.Nombre,
            Permisos: PermisoCatalogo.ToNombres(usuario.Rol.Permisos),
            RecursoId: recurso?.Id
        );
    }

    // -------------------------------------------------------------------------
    // Mapeo privado centralizado — un solo lugar para cambiar si el DTO evoluciona.
    // -------------------------------------------------------------------------

    private static UsuarioDto MapToDto(Usuario usuario) => new(
        Id: usuario.Id,
        Nombre: usuario.Nombre,
        Email: usuario.Email,
        RolId: usuario.RolId,
        RolNombre: usuario.Rol.Nombre,
        Activo: usuario.Activo
    );
}
