using System;
using System.Collections.Generic;
using System.Text;
using TurnosApp.Core.Domain.Entities;

namespace TurnosApp.Core.Application.Interfaces.Persistence;

/// <summary>
/// Repositorio específico de MetodoPago.
/// Extiende el genérico para permitir queries complejas a futuro.
/// </summary>
public interface IMetodoPagoRepository : IRepository<MetodoPago>
{
}
