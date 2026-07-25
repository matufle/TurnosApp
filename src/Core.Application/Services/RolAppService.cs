using System;
using System.Collections.Generic;
using System.Text;
using TurnosApp.Core.Application.Common;
using TurnosApp.Core.Application.DTOs.Roles;
using TurnosApp.Core.Application.Exceptions;
using TurnosApp.Core.Application.Interfaces.Persistence;
using TurnosApp.Core.Application.Interfaces.Services;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Core.Exceptions;

namespace TurnosApp.Core.Application.Services;

public class RolAppService : IRolAppService
{
    private readonly IUnitOfWork _unitOfWork;

    public RolAppService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<RolDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var rol = await _unitOfWork.Roles.GetByIdAsync(id, cancellationToken);

        if (rol is null)
            throw new NotFoundException(nameof(Rol), id);

        return MapToDto(rol);
    }

    public async Task<IReadOnlyList<RolDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var roles = await _unitOfWork.Roles.GetAllAsync(cancellationToken);

        return roles.Select(MapToDto).ToList();
    }

    public async Task<RolDto> CreateAsync(CreateRolDto dto, CancellationToken cancellationToken = default)
    {
        var rol = new Rol
        {
            Nombre = dto.Nombre,
            Permisos = PermisoCatalogo.Parse(dto.Permisos),
            EsSistema = false
        };

        await _unitOfWork.Roles.AddAsync(rol, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(rol);
    }

    public async Task<RolDto> UpdateAsync(int id, UpdateRolDto dto, CancellationToken cancellationToken = default)
    {
        var rol = await _unitOfWork.Roles.GetByIdAsync(id, cancellationToken);

        if (rol is null)
            throw new NotFoundException(nameof(Rol), id);

        if (rol.EsSistema)
            throw new BusinessException("ROL_SISTEMA_NO_EDITABLE", "El rol Admin no se puede editar.");

        rol.Nombre = dto.Nombre;
        rol.Permisos = PermisoCatalogo.Parse(dto.Permisos);

        _unitOfWork.Roles.Update(rol);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(rol);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var rol = await _unitOfWork.Roles.GetByIdAsync(id, cancellationToken);

        if (rol is null)
            throw new NotFoundException(nameof(Rol), id);

        if (rol.EsSistema)
            throw new BusinessException("ROL_SISTEMA_NO_ELIMINABLE", "El rol Admin no se puede eliminar.");

        var usuariosConEsteRol = await _unitOfWork.Usuarios.ContarPorRolAsync(id, cancellationToken);
        if (usuariosConEsteRol > 0)
            throw new BusinessException("ROL_CON_USUARIOS_ASIGNADOS", "No se puede eliminar un rol con usuarios asignados.");

        _unitOfWork.Roles.Delete(rol);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // -------------------------------------------------------------------------
    // Mapeo privado centralizado — un solo lugar para cambiar si el DTO evoluciona.
    // -------------------------------------------------------------------------

    private static RolDto MapToDto(Rol rol) => new(
        Id: rol.Id,
        Nombre: rol.Nombre,
        EsSistema: rol.EsSistema,
        Permisos: PermisoCatalogo.ToNombres(rol.Permisos)
    );
}
