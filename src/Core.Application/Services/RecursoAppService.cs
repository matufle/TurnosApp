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
        return recursos.Select(MapToDto).ToList();
    }

    public async Task<RecursoDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var recurso = await _unitOfWork.Recursos.GetByIdAsync(id, cancellationToken);

        if (recurso is null)
            throw new NotFoundException(nameof(Recurso), id);

        return MapToDto(recurso);
    }

    public async Task<RecursoDto> CreateAsync(CreateRecursoDto dto, CancellationToken cancellationToken = default)
    {
        var recurso = new Recurso
        {
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            Activo = true,
            ColorHex = dto.ColorHex ?? "#0EA5E9"
        };

        await _unitOfWork.Recursos.AddAsync(recurso, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(recurso);
    }

    public async Task<RecursoDto> UpdateAsync(int id, UpdateRecursoDto dto, CancellationToken cancellationToken = default)
    {
        var recurso = await _unitOfWork.Recursos.GetByIdAsync(id, cancellationToken);

        if (recurso is null)
            throw new NotFoundException(nameof(Recurso), id);

        recurso.Nombre = dto.Nombre;
        recurso.Descripcion = dto.Descripcion;
        recurso.Activo = dto.Activo;
        recurso.ColorHex = dto.ColorHex ?? "#0EA5E9";

        _unitOfWork.Recursos.Update(recurso);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(recurso);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var recurso = await _unitOfWork.Recursos.GetByIdAsync(id, cancellationToken);

        if (recurso is null)
            throw new NotFoundException(nameof(Recurso), id);

        _unitOfWork.Recursos.Delete(recurso);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static RecursoDto MapToDto(Recurso r) => new(
        Id: r.Id,
        Nombre: r.Nombre,
        Descripcion: r.Descripcion,
        Activo: r.Activo,
        ColorHex: r.ColorHex
    );
}