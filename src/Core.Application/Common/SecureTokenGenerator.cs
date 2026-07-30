using System.Security.Cryptography;

namespace TurnosApp.Core.Application.Common;

public static class SecureTokenGenerator
{
    public static string Generar()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
