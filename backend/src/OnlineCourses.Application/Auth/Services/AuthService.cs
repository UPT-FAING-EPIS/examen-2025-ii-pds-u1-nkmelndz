using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using OnlineCourses.Application.Auth.DTOs;
using OnlineCourses.Domain.Entities;
using System.Collections.Concurrent;
using System.Net.Mail;
using BCrypt.Net; // Paquete BCrypt.Net-Next

// Alias explícito por si el analizador no resuelve correctamente el espacio de nombres completo
using BcryptNet = BCrypt.Net.BCrypt;

namespace OnlineCourses.Application.Auth.Services;

public class AuthService
{
    // Al ser un servicio registrado como Singleton en Program.cs necesitamos seguridad de concurrencia
    private readonly ConcurrentDictionary<string, User> _users = new(); // key: email normalizado (lower)
    private readonly string _jwtKey;
    private readonly string? _issuer;
    private readonly string? _audience;
    private readonly TimeSpan _tokenLifetime;

    public AuthService(string key, string? issuer, string? audience, int hours = 4)
    {
        _jwtKey = string.IsNullOrWhiteSpace(key) ? "DEV_SECRET_KEY_CHANGE" : key;
        _issuer = issuer;
        _audience = audience;
        if (hours <= 0) hours = 4;
        _tokenLifetime = TimeSpan.FromHours(hours);
    }

    public AuthResponse Register(RegisterRequest req)
    {
        // Validaciones básicas
        var email = (req.Email ?? string.Empty).Trim();
        var fullName = (req.FullName ?? string.Empty).Trim();
        var password = req.Password ?? string.Empty;

        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email requerido");
        if (!EsEmailValido(email)) throw new ArgumentException("Email inválido");
        if (string.IsNullOrWhiteSpace(fullName)) throw new ArgumentException("Nombre requerido");
        if (password.Length < 6) throw new ArgumentException("Password mínimo 6 caracteres");

        var key = email.ToLowerInvariant();
        if (_users.ContainsKey(key)) throw new InvalidOperationException("Email already registered");

        var user = new User
        {
            Email = email,
            FullName = fullName,
            PasswordHash = BcryptNet.HashPassword(password),
            Role = "Student"
        };

        if (!_users.TryAdd(key, user))
            throw new InvalidOperationException("No se pudo registrar el usuario (race condition)");

        return GenerateToken(user);
    }

    public AuthResponse Login(LoginRequest req)
    {
        var email = (req.Email ?? string.Empty).Trim();
        var password = req.Password ?? string.Empty;
        var key = email.ToLowerInvariant();
        if (!_users.TryGetValue(key, out var user) || user is null || !BcryptNet.Verify(password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials");
        return GenerateToken(user!);
    }

    private AuthResponse GenerateToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtKey);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, user.Role)
        };
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.Add(_tokenLifetime),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = _issuer,
            Audience = _audience
        };
        var token = tokenHandler.CreateToken(descriptor);
        var jwt = tokenHandler.WriteToken(token);
        return new AuthResponse(jwt, user.Email, user.FullName, user.Role);
    }

    private static bool EsEmailValido(string email)
    {
        try
        {
            _ = new MailAddress(email);
            return true;
        }
        catch { return false; }
    }
}
