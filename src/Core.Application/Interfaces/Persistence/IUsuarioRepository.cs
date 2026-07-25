using System;
using System.Collections.Generic;
using System.Text;
using TurnosApp.Core.Domain.Entities;

public interface IUsuarioRepository
{
    Task<Usuario?> GetByEmailGlobalAsync(string email, CancellationToken cancellationToken);
    Task<Usuario?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Usuario>> GetAllAsync(CancellationToken cancellationToken);
    Task<int> ContarPorRolAsync(int rolId, CancellationToken cancellationToken);
    Task AddAsync(Usuario usuario, CancellationToken cancellationToken);
}
