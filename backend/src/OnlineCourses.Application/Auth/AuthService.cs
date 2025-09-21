using System.Security.Cryptography;
using System.Text;
using OnlineCourses.Application.Auth.Dtos;
using OnlineCourses.Domain.Entities;

namespace OnlineCourses.Application.Auth;

public class AuthService
{
    private readonly IUserRepository _users;
    private readonly ITokenGenerator _tokens;

    public AuthService(IUserRepository users, ITokenGenerator tokens)
    {
        _users = users; _tokens = tokens;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest req, CancellationToken ct = default)
    {
        if (req is null) throw new ArgumentNullException(nameof(req));
        if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password) || string.IsNullOrWhiteSpace(req.FullName))
            throw new ArgumentException("Email, Password y FullName son obligatorios");
        var existing = await _users.GetByEmailAsync(req.Email, ct);
        if (existing != null) throw new InvalidOperationException("Email ya registrado");
        var user = new User
        {
            Email = req.Email.Trim().ToLowerInvariant(),
            PasswordHash = HashPassword(req.Password),
            FullName = req.FullName,
            Role = UserRole.Student
        };
        await _users.AddAsync(user, ct);
        var token = _tokens.Generate(user);
        return new AuthResponse(user.Id, user.Email, user.FullName, user.Role.ToString(), token);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest req, CancellationToken ct = default)
    {
        var user = await _users.GetByEmailAsync(req.Email.Trim().ToLowerInvariant(), ct);
        if (user == null || !VerifyPassword(req.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Credenciales inválidas");
        var token = _tokens.Generate(user);
        return new AuthResponse(user.Id, user.Email, user.FullName, user.Role.ToString(), token);
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    private static bool VerifyPassword(string password, string storedHash)
        => HashPassword(password) == storedHash; // (Luego sustituir por BCrypt)
}

public interface ITokenGenerator
{
    string Generate(User user);
}