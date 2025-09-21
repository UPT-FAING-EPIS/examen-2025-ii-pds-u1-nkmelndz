using System.ComponentModel.DataAnnotations;

namespace OnlineCourses.Application.Auth.DTOs;

/// <summary>
/// Credenciales para autenticación.
/// </summary>
public class LoginRequest
{
	[Required, EmailAddress, MaxLength(200)]
	public string Email { get; set; } = string.Empty;

	[Required, MinLength(6), MaxLength(100)]
	public string Password { get; set; } = string.Empty;
}