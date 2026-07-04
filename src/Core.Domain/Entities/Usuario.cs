namespace TurnosApp.Core.Domain.Entities;

public class Usuario
{
    public int Id { get; private set; }
    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public int TenantId { get; private set; }

    private Usuario() { } // EF Core

    //Trabajamos siempre con la forma hash de la contraseña, nunca con la contraseña en texto plano
    public Usuario(string email, string passwordHash, int tenantId)
    {
        Email = email.ToLowerInvariant().Trim();
        PasswordHash = passwordHash;
        TenantId = tenantId;
    }

    public void ActualizarPasswordHash(string nuevoHash)
    {
        PasswordHash = nuevoHash;
    }
}