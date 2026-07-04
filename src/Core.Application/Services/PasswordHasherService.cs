using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity;
using TurnosApp.Core.Application.Interfaces.Services;
using TurnosApp.Core.Domain.Entities;

namespace TurnosApp.Core.Application.Services;

public class PasswordHasherService : IPasswordHasherService
{
    private readonly PasswordHasher<Usuario> _hasher = new();

    public string HashPassword(string password)
        => _hasher.HashPassword(null!, password);


    // Verifica si la contraseña proporcionada coincide con el hash almacenado.
    public bool VerifyPassword(string hashedPassword, string providedPassword)
        => _hasher.VerifyHashedPassword(null!, hashedPassword, providedPassword)
            != PasswordVerificationResult.Failed;
}
