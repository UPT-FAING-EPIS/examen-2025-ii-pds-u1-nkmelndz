using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using OnlineCourses.Application.Auth;
using OnlineCourses.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace OnlineCourses.Infrastructure.Auth;

public class SimpleJwtGenerator : ITokenGenerator
{
    private readonly string _key;
    private readonly string _issuer;
    private readonly string _audience;

    public SimpleJwtGenerator(IConfiguration config)
    {
        _key = config["Jwt:Key"] ?? "dev-secret-unsafe-change-me-32bytes-min-length-123"; // >= 32 bytes
        _issuer = config["Jwt:Issuer"] ?? "OnlineCourses";
        _audience = config["Jwt:Audience"] ?? "OnlineCoursesAudience";
        if (Encoding.UTF8.GetByteCount(_key) < 32)
        {
            throw new InvalidOperationException("JWT Key configurada es demasiado corta. Debe tener al menos 32 bytes para HS256.");
        }
    }

    public string Generate(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("fullName", user.FullName)
        };

        var keyBytes = Encoding.UTF8.GetBytes(_key);
        // Seguridad defensiva: aunque ya validamos en ctor, revalidar antes de usar
        if (keyBytes.Length < 32)
            throw new InvalidOperationException("JWT Key es demasiado corta (revalidación). Configure Jwt:Key >= 32 bytes.");
        var key = new SymmetricSecurityKey(keyBytes);
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}