using System.ComponentModel.DataAnnotations;

namespace TurnosApp.Core.Application.DTOs;

public record ReenviarConfirmacionDto([Required, EmailAddress, StringLength(256)] string Email);
