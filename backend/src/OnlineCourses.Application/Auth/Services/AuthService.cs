using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using OnlineCourses.Application.Auth.DTOs;
using OnlineCourses.Domain.Entities;

namespace OnlineCourses.Application.Auth.Services;

public class AuthService
{
    private readonly List<User> _users = new(); // InMemory temporal
    private readonly string _jwtKey;

    public AuthService(IConfiguration configuration)
    {
        _jwtKey = configuration["Jwt:Key"] ?? "DEV_SECRET_KEY_CHANGE";
    }

    public AuthResponse Register(RegisterRequest req)
    {
        if (_users.Any(u => u.Email.Equals(req.Email, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Email already registered");
        var user = new User
        {
            Email = req.Email,
            FullName = req.FullName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            Role = "Student"
        };
        _users.Add(user);
        return GenerateToken(user);
    }

    public AuthResponse Login(LoginRequest req)
    {
        var user = _users.FirstOrDefault(u => u.Email.Equals(req.Email, StringComparison.OrdinalIgnoreCase));
        if (user == null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials");
        return GenerateToken(user);
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
            Expires = DateTime.UtcNow.AddHours(4),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(descriptor);
        var jwt = tokenHandler.WriteToken(token);
        return new AuthResponse(jwt, user.Email, user.FullName, user.Role);
    }
}
