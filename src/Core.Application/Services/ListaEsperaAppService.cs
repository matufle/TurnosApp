using TurnosApp.Core.Application.DTOs.ListaEspera;
using TurnosApp.Core.Application.Exceptions;
using TurnosApp.Core.Application.Interfaces.Persistence;
using TurnosApp.Core.Application.Interfaces.Services;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Core.Domain.Enums;
using TurnosApp.Core.Exceptions;

namespace TurnosApp.Core.Application.Services;

public class ListaEsperaAppService : IListaEsperaAppService
{
    private readonly IUnitOfWork _unitOfWork;

    public ListaEsperaAppService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ListaEsperaDto> CrearAsync(CrearListaEsperaDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.FechaHasta <= dto.FechaDesde)
            throw new BadRequestException("FechaHasta debe ser posterior a FechaDesde.");

        var cliente = await _unitOfWork.Clientes.GetByIdAsync(dto.ClienteId, cancellationToken)
            ?? throw new NotFoundException(nameof(Cliente), dto.ClienteId);

        var recurso = await _unitOfWork.Recursos.GetByIdAsync(dto.RecursoId, cancellationToken)
            ?? throw new NotFoundException(nameof(Recurso), dto.RecursoId);

        Servicio? servicio = null;
        if (dto.ServicioId.HasValue)
        {
            servicio = await _unitOfWork.Servicios.GetByIdAsync(dto.ServicioId.Value, cancellationToken)
                ?? throw new NotFoundException(nameof(Servicio), dto.ServicioId.Value);
        }

        var listaEspera = new ListaEspera
        {
            ClienteId = cliente.Id,
            Cliente = cliente,
            RecursoId = recurso.Id,
            Recurso = recurso,
            ServicioId = servicio?.Id,
            Servicio = servicio,
            FechaDesde = dto.FechaDesde,
            FechaHasta = dto.FechaHasta,
            Estado = EstadoListaEspera.Activa
        };

        await _unitOfWork.ListasEspera.AddAsync(listaEspera, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(listaEspera);
    }

    public async Task<IReadOnlyList<ListaEsperaDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entradas = await _unitOfWork.ListasEspera.GetAllConDetallesAsync(cancellationToken);
        return entradas.Select(MapToDto).ToList();
    }

    public async Task<ListaEsperaDto> CancelarAsync(int id, CancellationToken cancellationToken = default)
    {
        var listaEspera = await _unitOfWork.ListasEspera.GetByIdConDetallesAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(ListaEspera), id);

        if (listaEspera.Estado == EstadoListaEspera.Cancelada)
            throw new BusinessException("LISTA_ESPERA_YA_CANCELADA", "Esta entrada ya fue cancelada.");

        listaEspera.Estado = EstadoListaEspera.Cancelada;

        _unitOfWork.ListasEspera.Update(listaEspera);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(listaEspera);
    }

    private static ListaEsperaDto MapToDto(ListaEspera l) => new(
        Id: l.Id,
        ClienteId: l.ClienteId,
        ClienteNombreCompleto: $"{l.Cliente.Nombre} {l.Cliente.Apellido}",
        RecursoId: l.RecursoId,
        RecursoNombre: l.Recurso.Nombre,
        ServicioId: l.ServicioId,
        ServicioNombre: l.Servicio?.Nombre,
        FechaDesde: l.FechaDesde,
        FechaHasta: l.FechaHasta,
        Estado: l.Estado.ToString(),
        NotificadoEn: l.NotificadoEn
    );
}
