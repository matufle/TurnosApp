using System;
using System.Collections.Generic;
using System.Text;
using TurnosApp.Core.Application.DTOs.MetodosPago;
using TurnosApp.Core.Application.Interfaces.Persistence;
using TurnosApp.Core.Application.Interfaces.Services;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Core.Domain.Enums;
using TurnosApp.Core.Exceptions;

namespace TurnosApp.Core.Application.Services;

public class MetodoPagoService : IMetodoPagoService
{
    private readonly IUnitOfWork _unitOfWork;

    public MetodoPagoService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<MetodoPagoDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var metodoPago = await _unitOfWork.MetodoPagos.GetByIdAsync(id, cancellationToken);

        if (metodoPago is null)
            throw new NotFoundException(nameof(MetodoPago), id);

        return MapToDto(metodoPago);
    }

    public async Task<IReadOnlyList<MetodoPagoDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var metodosPago = await _unitOfWork.MetodoPagos.GetAllAsync(cancellationToken);

        return metodosPago.Select(MapToDto).ToList();
    }

    public async Task<MetodoPagoDto> CreateAsync(CreateMetodoPagoDto dto, CancellationToken cancellationToken = default)
    {
        var tipo = ParseTipoModificador(dto.TipoModificador);

        var metodoPago = new MetodoPago
        {
            Nombre = dto.Nombre,
            TipoModificador = tipo,
            PorcentajeModificador = dto.PorcentajeModificador,
            PorcentajeComision = dto.PorcentajeComision,
            EsEfectivo = dto.EsEfectivo,
            Activo = true
        };

        await _unitOfWork.MetodoPagos.AddAsync(metodoPago, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(metodoPago);
    }

    public async Task<MetodoPagoDto> UpdateAsync(int id, UpdateMetodoPagoDto dto, CancellationToken cancellationToken = default)
    {
        var metodoPago = await _unitOfWork.MetodoPagos.GetByIdAsync(id, cancellationToken);

        if (metodoPago is null)
            throw new NotFoundException(nameof(MetodoPago), id);

        metodoPago.Nombre = dto.Nombre;
        metodoPago.TipoModificador = ParseTipoModificador(dto.TipoModificador);
        metodoPago.PorcentajeModificador = dto.PorcentajeModificador;
        metodoPago.PorcentajeComision = dto.PorcentajeComision;
        metodoPago.Activo = dto.Activo;
        metodoPago.EsEfectivo = dto.EsEfectivo;

        _unitOfWork.MetodoPagos.Update(metodoPago);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(metodoPago);
    }

    public async Task<MetodoPagoDto> DesactivarAsync(int id, CancellationToken cancellationToken = default)
    {
        var metodoPago = await _unitOfWork.MetodoPagos.GetByIdAsync(id, cancellationToken);

        if (metodoPago is null)
            throw new NotFoundException(nameof(MetodoPago), id);

        // Soft delete: NUNCA hard-delete, para no romper la FK histórica de Cobro.MetodoPagoId.
        metodoPago.Activo = false;

        _unitOfWork.MetodoPagos.Update(metodoPago);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(metodoPago);
    }

    private static TipoModificadorPago ParseTipoModificador(string valor)
    {
        if (!Enum.TryParse<TipoModificadorPago>(valor, ignoreCase: true, out var tipo))
            throw new BadRequestException($"'{valor}' no es un tipo de modificador válido.");

        return tipo;
    }

    private static MetodoPagoDto MapToDto(MetodoPago m) => new(
        Id: m.Id,
        Nombre: m.Nombre,
        TipoModificador: m.TipoModificador.ToString(),
        PorcentajeModificador: m.PorcentajeModificador,
        PorcentajeComision: m.PorcentajeComision,
        Activo: m.Activo,
        EsEfectivo: m.EsEfectivo
    );
}
