namespace TurnosApp.Core.Domain.Entities;

public class Usuario
{
    public int Id { get; private set; }
    public string Nombre { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public int TenantId { get; private set; }
    public int RolId { get; private set; }
    public Rol Rol { get; private set; } = null!;
    public bool Activo { get; private set; } = true;
    public bool OnboardingCompletado { get; private set; } = false;
    public bool EmailConfirmado { get; private set; } = false;
    public string? TokenConfirmacionEmail { get; private set; }
    public DateTime? TokenConfirmacionExpira { get; private set; }
    public string? TokenResetPassword { get; private set; }
    public DateTime? TokenResetPasswordExpira { get; private set; }

    private Usuario() { } // EF Core

    //Trabajamos siempre con la forma hash de la contraseña, nunca con la contraseña en texto plano
    public Usuario(string nombre, string email, string passwordHash, int tenantId, int rolId)
    {
        Nombre = nombre.Trim();
        Email = email.ToLowerInvariant().Trim();
        PasswordHash = passwordHash;
        TenantId = tenantId;
        RolId = rolId;
        Activo = true;
    }

    public void ActualizarPasswordHash(string nuevoHash)
    {
        PasswordHash = nuevoHash;
    }

    public void AsignarRol(int rolId)
    {
        RolId = rolId;
    }

    public void ActualizarNombre(string nombre)
    {
        Nombre = nombre.Trim();
    }

    public void Activar() => Activo = true;

    public void Desactivar() => Activo = false;

    public void CompletarOnboarding() => OnboardingCompletado = true;

    public void EstablecerTokenConfirmacion(string token, DateTime expira)
    {
        TokenConfirmacionEmail = token;
        TokenConfirmacionExpira = expira;
    }

    public void ConfirmarEmail()
    {
        EmailConfirmado = true;
        TokenConfirmacionEmail = null;
        TokenConfirmacionExpira = null;
    }

    public void EstablecerTokenResetPassword(string token, DateTime expira)
    {
        TokenResetPassword = token;
        TokenResetPasswordExpira = expira;
    }

    public void LimpiarTokenResetPassword()
    {
        TokenResetPassword = null;
        TokenResetPasswordExpira = null;
    }
}