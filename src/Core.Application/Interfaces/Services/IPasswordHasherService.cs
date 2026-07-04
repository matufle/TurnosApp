using System;
using System.Collections.Generic;
using System.Text;

namespace TurnosApp.Core.Application.Interfaces.Services;

public interface IPasswordHasherService
{
    string HashPassword(string password);
    bool VerifyPassword(string hashedPassword, string providedPassword);
}
