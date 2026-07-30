using TurnosApp.Core.Application.DTOs.ReglasComision;
using TurnosApp.Core.Application.Exceptions;
using TurnosApp.Core.Application.Interfaces.Persistence;
using TurnosApp.Core.Application.Interfaces.Services;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Core.Domain.Enums;
using TurnosApp.Core.Exceptions;

namespace TurnosApp.Core.Application.Services;

public class ReglaComisionAppService : IReglaComisionAppService
{
    private readonly IUnitOfWork _unitOfWork;

    public ReglaComisionAppService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<ReglaComisionDto>> GetByRecursoAsync(int recursoId, CancellationToken cancellationToken = default)
    {
        var reglas = await _unitOfWork.ReglasComision.GetByRecursoAsync(recursoId, cancellationToken);
        return reglas.Select(MapToDto).ToList();
    }

    public async Task<ReglaComisionDto> CreateAsync(CreateReglaComisionDto dto, CancellationToken cancellationToken = default)
    {
        var recurso = await _unitOfWork.Recursos.GetByIdAsync(dto.RecursoId, cancellationToken);
        if (recurso is null)
            throw new NotFoundException(nameof(Recurso), dto.RecursoId);

        if (dto.ServicioId is int servicioId)
        {
            var servicio = await _unitOfWork.Servicios.GetByIdAsync(servicioId, cancellationToken);
            if (servicio is null)
                throw new NotFoundException(nameof(Servicio), servicioId);
        }

        var tipo = ParseTipo(dto.Tipo);
        ValidarValor(tipo, dto.Valor);

        var existentes = await _unitOfWork.ReglasComision.GetByRecursoAsync(dto.RecursoId, cancellationToken);
        if (existentes.Any(r => r.Activo && r.ServicioId == dto.ServicioId))
            throw new BusinessException(
                code: "REGLA_COMISION_YA_EXISTE",
                message: dto.ServicioId is null
                    ? "Ya existe una regla de comisión base activa para este profesional."
                    : "Ya existe una regla de comisión activa para este profesional y servicio.");

        var regla = new ReglaComision
        {
            RecursoId = dto.RecursoId,
            ServicioId = dto.ServicioId,
            Tipo = tipo,
            Valor = dto.Valor,
            Activo = true
        };

        await _unitOfWork.ReglasComision.AddAsync(regla, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        regla.Recurso = recurso;
        return MapToDto(regla);
    }

    public async Task<ReglaComisionDto> UpdateAsync(int id, UpdateReglaComisionDto dto, CancellationToken cancellationToken = default)
    {
        var regla = await _unitOfWork.ReglasComision.GetByIdAsync(id, cancellationToken);
        if (regla is null)
            throw new NotFoundException(nameof(ReglaComision), id);

        var tipo = ParseTipo(dto.Tipo);
        ValidarValor(tipo, dto.Valor);

        // Se reusa para el chequeo de duplicado al reactivar y para traer Servicio (GetByIdAsync
        // no incluye navegaciones) de cara al mapeo de ServicioNombre en la respuesta.
        var hermanas = await _unitOfWork.ReglasComision.GetByRecursoAsync(regla.RecursoId, cancellationToken);

        if (dto.Activo && !regla.Activo && hermanas.Any(r => r.Id != id && r.Activo && r.ServicioId == regla.ServicioId))
            throw new BusinessException(
                code: "REGLA_COMISION_YA_EXISTE",
                message: regla.ServicioId is null
                    ? "Ya existe una regla de comisión base activa para este profesional."
                    : "Ya existe una regla de comisión activa para este profesional y servicio.");

        regla.Tipo = tipo;
        regla.Valor = dto.Valor;
        regla.Activo = dto.Activo;

        _unitOfWork.ReglasComision.Update(regla);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        regla.Servicio = hermanas.FirstOrDefault(r => r.Id == id)?.Servicio;
        return MapToDto(regla);
    }

    private static TipoComision ParseTipo(string valor)
    {
        if (!Enum.TryParse<TipoComision>(valor, ignoreCase: true, out var tipo))
            throw new BadRequestException($"'{valor}' no es un tipo de comisión válido.");

        return tipo;
    }

    private static void ValidarValor(TipoComision tipo, decimal valor)
    {
        if (valor < 0)
            throw new BusinessException(code: "VALOR_INVALIDO", message: "El valor de la comisión no puede ser negativo.");

        if (tipo == TipoComision.Porcentaje && valor > 100)
            throw new BusinessException(code: "VALOR_INVALIDO", message: "Un porcentaje de comisión no puede superar el 100%.");
    }

    private static ReglaComisionDto MapToDto(ReglaComision r) => new(
        Id: r.Id,
        RecursoId: r.RecursoId,
        ServicioId: r.ServicioId,
        ServicioNombre: r.Servicio?.Nombre,
        Tipo: r.Tipo.ToString(),
        Valor: r.Valor,
        Activo: r.Activo
    );
}
