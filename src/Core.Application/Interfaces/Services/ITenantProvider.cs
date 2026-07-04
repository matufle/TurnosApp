using System;
using System.Collections.Generic;
using System.Text;

namespace TurnosApp.Core.Application.Interfaces.Services;

public interface ITenantProvider
{
    int GetCurrentTenantId();
}