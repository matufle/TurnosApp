using System;
using System.Collections.Generic;
using System.Text;
using TurnosApp.Core.Application.DTOs.Recursos;
using TurnosApp.Core.Application.Exceptions;
using TurnosApp.Core.Application.Interfaces.Persistence;
using TurnosApp.Core.Application.Interfaces.Services;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Core.Exceptions;

namespace TurnosApp.Core.Application.Services;

public class RecursoAppService : IRecursoAppService
{
    private readonly IUnitOfWork _unitOfWork;

    public RecursoAppService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<RecursoDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var recursos = await _unitOfWork.Recursos.GetAllAsync(cancellationToken);
        var usuariosPorId = (await _unitOfWork.Usuarios.GetAllAsync(cancellationToken))
            .ToDictionary(u => u.Id, u => u.Nombre);

        return recursos
            .Select(r => MapToDto(r, r.UsuarioId.HasValue && usuariosPorId.TryGetValue(r.UsuarioId.Value, out var nombre) ? nombre : null))
            .ToList();
    }

    public async Task<RecursoDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var recurso = await _unitOfWork.Recursos.GetByIdAsync(id, cancellationToken);

        if (recurso is null)
            throw new NotFoundException(nameof(Recurso), id);

        var usuarioNombre = await ObtenerNombreUsuarioAsync(recurso.UsuarioId, cancellationToken);
        return MapToDto(recurso, usuarioNombre);
    }

    public async Task<RecursoDto> CreateAsync(CreateRecursoDto dto, CancellationToken cancellationToken = default)
    {
        var usuario = await ValidarYObtenerUsuarioParaVincularAsync(dto.UsuarioId, recursoIdActual: null, cancellationToken);

        var recurso = new Recurso
        {
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            Activo = true,
            ColorHex = dto.ColorHex ?? "#0EA5E9",
            UsuarioId = dto.UsuarioId
        };

        await _unitOfWork.Recursos.AddAsync(recurso, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(recurso, usuario?.Nombre);
    }

    public async Task<RecursoDto> UpdateAsync(int id, UpdateRecursoDto dto, CancellationToken cancellationToken = default)
    {
        var recurso = await _unitOfWork.Recursos.GetByIdAsync(id, cancellationToken);

        if (recurso is null)
            throw new NotFoundException(nameof(Recurso), id);

        var usuario = await ValidarYObtenerUsuarioParaVincularAsync(dto.UsuarioId, recursoIdActual: id, cancellationToken);

        recurso.Nombre = dto.Nombre;
        recurso.Descripcion = dto.Descripcion;
        recurso.Activo = dto.Activo;
        recurso.ColorHex = dto.ColorHex ?? "#0EA5E9";
        recurso.UsuarioId = dto.UsuarioId;

        _unitOfWork.Recursos.Update(recurso);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(recurso, usuario?.Nombre);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var recurso = await _unitOfWork.Recursos.GetByIdAsync(id, cancellationToken);

        if (recurso is null)
            throw new NotFoundException(nameof(Recurso), id);

        _unitOfWork.Recursos.Delete(recurso);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UsuarioParaVincularDto>> GetUsuariosDisponiblesAsync(int? recursoIdActual, CancellationToken cancellationToken = default)
    {
        var usuarios = await _unitOfWork.Usuarios.GetAllAsync(cancellationToken);
        var recursos = await _unitOfWork.Recursos.GetAllAsync(cancellationToken);

        // Un usuario ya vinculado a OTRO recurso no puede volver a elegirse (índice único
        // en Recursos.UsuarioId); el vinculado al recurso que se está editando sí se re-incluye
        // para que la edición no lo saque de la lista.
        var usuariosYaVinculados = recursos
            .Where(r => r.UsuarioId.HasValue && r.Id != recursoIdActual)
            .Select(r => r.UsuarioId!.Value)
            .ToHashSet();

        return usuarios
            .Where(u => u.Activo && !usuariosYaVinculados.Contains(u.Id))
            .Select(u => new UsuarioParaVincularDto(u.Id, u.Nombre))
            .ToList();
    }

    private async Task<Usuario?> ValidarYObtenerUsuarioParaVincularAsync(int? usuarioId, int? recursoIdActual, CancellationToken cancellationToken)
    {
        if (!usuarioId.HasValue)
            return null;

        var usuario = await _unitOfWork.Usuarios.GetByIdAsync(usuarioId.Value, cancellationToken)
            ?? throw new NotFoundException(nameof(Usuario), usuarioId.Value);

        var recursoVinculado = await _unitOfWork.Recursos.GetByUsuarioIdAsync(usuarioId.Value, cancellationToken);
        if (recursoVinculado is not null && recursoVinculado.Id != recursoIdActual)
            throw new ConflictException("Ese usuario ya está vinculado a otro recurso.");

        return usuario;
    }

    private async Task<string?> ObtenerNombreUsuarioAsync(int? usuarioId, CancellationToken cancellationToken)
    {
        if (!usuarioId.HasValue)
            return null;

        var usuario = await _unitOfWork.Usuarios.GetByIdAsync(usuarioId.Value, cancellationToken);
        return usuario?.Nombre;
    }

    private static RecursoDto MapToDto(Recurso r, string? usuarioNombre) => new(
        Id: r.Id,
        Nombre: r.Nombre,
        Descripcion: r.Descripcion,
        Activo: r.Activo,
        ColorHex: r.ColorHex,
        UsuarioId: r.UsuarioId,
        UsuarioNombre: usuarioNombre
    );
}