namespace OnlineCourses.Application.Auth.DTOs;

/// <summary>
/// Respuesta de autenticación con token JWT emitido.
/// </summary>
public class AuthResponse
{
	public string Token { get; init; } = string.Empty;
	public string Email { get; init; } = string.Empty;
	public string FullName { get; init; } = string.Empty;
	public string Role { get; init; } = string.Empty;

	public AuthResponse() { }
	public AuthResponse(string token, string email, string fullName, string role)
	{
		Token = token;
		Email = email;
		FullName = fullName;
		Role = role;
	}
}