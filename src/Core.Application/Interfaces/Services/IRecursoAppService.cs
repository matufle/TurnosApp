using System;
using System.Collections.Generic;
using System.Text;
using TurnosApp.Core.Domain.Entities;

using TurnosApp.Core.Application.DTOs.Recursos;

namespace TurnosApp.Core.Application.Interfaces.Services;

public interface IRecursoAppService
{
    Task<IReadOnlyList<RecursoDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<RecursoDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<RecursoDto> CreateAsync(CreateRecursoDto dto, CancellationToken cancellationToken = default);
    Task<RecursoDto> UpdateAsync(int id, UpdateRecursoDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UsuarioParaVincularDto>> GetUsuariosDisponiblesAsync(int? recursoIdActual, CancellationToken cancellationToken = default);
}