using System.ComponentModel.DataAnnotations;

namespace TurnosApp.Core.Application.DTOs;

public record ConfirmarEmailDto([Required] string Token);
