using System.ComponentModel.DataAnnotations;

namespace OnlineCourses.Application.Auth.DTOs;

/// <summary>
/// Datos para registrar un nuevo usuario (rol por defecto: Student).
/// </summary>
public class RegisterRequest
{
	[Required, EmailAddress, MaxLength(200)]
	public string Email { get; set; } = string.Empty;

	[Required, MaxLength(150)]
	public string FullName { get; set; } = string.Empty;

	[Required, MinLength(6), MaxLength(100)]
	public string Password { get; set; } = string.Empty;
}