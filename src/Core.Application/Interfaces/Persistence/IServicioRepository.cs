using System;
using System.Collections.Generic;
using System.Text;
using TurnosApp.Core.Domain.Entities;

namespace TurnosApp.Core.Application.Interfaces.Persistence;

/// <summary>
/// Repositorio específico de Servicio.
/// Extiende el genérico para permitir queries complejas a futuro
/// (ej: GetServiciosActivosAsync, GetByRecursoAsync).
/// </summary>
public interface IServicioRepository : IRepository<Servicio>
{
}
