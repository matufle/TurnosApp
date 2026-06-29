using System;
using System.Collections.Generic;
using System.Text;
using TurnosApp.Core.Application.DTOs.Clientes;
using TurnosApp.Core.Application.Exceptions;
using TurnosApp.Core.Application.Interfaces.Persistence;
using TurnosApp.Core.Application.Interfaces.Services;
using TurnosApp.Core.Domain.Entities;

namespace TurnosApp.Core.Application.Services;

public class ClienteService : IClienteService
{
    private readonly IUnitOfWork _unitOfWork;

    public ClienteService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<ClienteDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var clientes = await _unitOfWork.Clientes.GetAllAsync(cancellationToken);
        return clientes.Select(MapToDto).ToList();
    }

    public async Task<ClienteDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var cliente = await _unitOfWork.Clientes.GetByIdAsync(id, cancellationToken);

        if (cliente is null)
            throw new NotFoundException(nameof(Cliente), id);

        return MapToDto(cliente);
    }

    public async Task<ClienteDto> CreateAsync(CreateClienteDto dto, CancellationToken cancellationToken = default)
    {
        var cliente = new Cliente
        {
            Nombre = dto.Nombre,
            Apellido = dto.Apellido,
            Email = dto.Email,
            Telefono = dto.Telefono,
            // NotasAdicionales se persiste en DatosEspecificosJson como string plano.
            // Cuando el dominio requiera JSON estructurado, este es el único punto a evolucionar.
            DatosEspecificosJson = dto.NotasAdicionales,
            Activo = true
        };

        await _unitOfWork.Clientes.AddAsync(cliente, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(cliente);
    }

    public async Task<ClienteDto> UpdateAsync(int id, UpdateClienteDto dto, CancellationToken cancellationToken = default)
    {
        var cliente = await _unitOfWork.Clientes.GetByIdAsync(id, cancellationToken);

        if (cliente is null)
            throw new NotFoundException(nameof(Cliente), id);

        cliente.Nombre = dto.Nombre;
        cliente.Apellido = dto.Apellido;
        cliente.Email = dto.Email;
        cliente.Telefono = dto.Telefono;
        cliente.DatosEspecificosJson = dto.NotasAdicionales;

        _unitOfWork.Clientes.Update(cliente);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(cliente);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var cliente = await _unitOfWork.Clientes.GetByIdAsync(id, cancellationToken);

        if (cliente is null)
            throw new NotFoundException(nameof(Cliente), id);

        _unitOfWork.Clientes.Delete(cliente);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // -------------------------------------------------------------------------

    private static ClienteDto MapToDto(Cliente cliente) => new(
        Id: cliente.Id,
        Nombre: cliente.Nombre,
        Apellido: cliente.Apellido,
        Email: cliente.Email,
        Telefono: cliente.Telefono,
        NotasAdicionales: cliente.DatosEspecificosJson
    );
}
