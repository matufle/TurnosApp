using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using TurnosApp.Core.Application.Interfaces.Services;
using TurnosApp.Core.Application.Services;

namespace TurnosApp.Core.Application.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IServicioService, ServicioService>();
        services.AddScoped<IClienteService, ClienteService>();
        services.AddScoped<ITurnoAppService, TurnoAppService>();

        // SolapamientoValidator como Scoped: necesita ITurnoRepository
        // que también es Scoped (vive dentro del UnitOfWork Scoped).
        services.AddScoped<SolapamientoValidator>();

        return services;
    }
}
