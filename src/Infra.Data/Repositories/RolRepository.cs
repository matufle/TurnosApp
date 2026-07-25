using System;
using System.Collections.Generic;
using System.Text;
using TurnosApp.Core.Application.Interfaces.Persistence;
using TurnosApp.Core.Domain.Entities;
using TurnosApp.Infra.Data.Context;

namespace TurnosApp.Infra.Data.Repositories;

public class RolRepository : GenericRepository<Rol>, IRolRepository
{
    public RolRepository(ApplicationDbContext context) : base(context)
    {
    }
}
