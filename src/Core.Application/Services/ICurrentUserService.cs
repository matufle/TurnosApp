using System;
using System.Collections.Generic;
using System.Text;

namespace TurnosApp.Core.Application.Interfaces.Services;

//Esto sirve para saber quien esta logueado
public interface ICurrentUserService
{
    int GetCurrentTenantId();
}