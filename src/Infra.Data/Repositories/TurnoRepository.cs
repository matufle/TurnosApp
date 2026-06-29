using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using TurnosApp.Core.Application.Interfaces.Persistence;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Core.Domain.Enums;
using TurnosApp.Infra.Data.Context;

namespace TurnosApp.Infra.Data.Repositories;

public class TurnoRepository : GenericRepository<Turno>, ITurnoRepository
{
    public TurnoRepository(ApplicationDbContext context) : base(context)
    {
    }

    // TurnoRepository.cs — versión completa para múltiples servicios
    public async Task<bool> ExisteTurnoEnRangoAsync(
        int recursoId, DateTime inicio, DateTime fin,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t =>
                t.RecursoId == recursoId &&
                (t.Estado == EstadoTurno.Pendiente || t.Estado == EstadoTurno.Confirmado))
            .Select(t => new
            {
                t.FechaHoraInicio,
                // FechaHoraFin calculada en SQL sumando duraciones de todos los servicios del turno
                FechaHoraFin = t.FechaHoraInicio.AddMinutes(
                    t.TurnoServicios.Sum(ts => ts.Servicio!.DuracionMinutos))
            })
            .Where(t =>
                inicio < t.FechaHoraFin &&   // el nuevo empieza antes de que termine el existente
                fin > t.FechaHoraInicio)  // el nuevo termina después de que empiece el existente
            .AnyAsync(cancellationToken);
    }
}