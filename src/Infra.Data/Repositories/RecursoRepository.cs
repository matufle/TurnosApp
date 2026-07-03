using System;
using System.Collections.Generic;
using System.Text;
using TurnosApp.Core.Application.Interfaces.Persistence;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Infra.Data.Context;

namespace TurnosApp.Infra.Data.Repositories;

public class RecursoRepository : GenericRepository<Recurso>, IRecursoRepository
{
    public RecursoRepository(ApplicationDbContext context) : base(context)
    {
    }
}