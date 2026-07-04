using System;
using System.Collections.Generic;
using System.Text;
using TurnosApp.Core.Domain.Entities;

public interface IUsuarioRepository
{
    Task<Usuario?> GetByEmailGlobalAsync(string email, CancellationToken cancellationToken);
    Task AddAsync(Usuario usuario, CancellationToken cancellationToken);
}
