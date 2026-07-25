using System;
using System.Collections.Generic;
using System.Text;

namespace TurnosApp.Core.Application.DTOs.Auth;

/// <summary>
/// Perfil del usuario autenticado + sus permisos resueltos, consumido por el frontend
/// para hidratar el AuthContext (nav, botones, y rutas protegidas por permiso).
/// </summary>
public record MeDto(
    int UsuarioId,
    string Nombre,
    string Email,
    int TenantId,
    int RolId,
    string RolNombre,
    IReadOnlyList<string> Permisos,
    int? RecursoId
);
