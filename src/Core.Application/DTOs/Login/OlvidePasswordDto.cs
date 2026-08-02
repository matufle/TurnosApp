using System.ComponentModel.DataAnnotations;

namespace TurnosApp.Core.Application.DTOs;

public record OlvidePasswordDto([Required, EmailAddress, StringLength(256)] string Email);
