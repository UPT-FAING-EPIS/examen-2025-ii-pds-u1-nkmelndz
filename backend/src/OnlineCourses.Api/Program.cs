using OnlineCourses.Application.Courses.Services;
using OnlineCourses.Domain.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using OnlineCourses.Application.Auth.Services;
using OnlineCourses.Application.Auth.DTOs;
using Microsoft.EntityFrameworkCore;
using OnlineCourses.Infrastructure.Persistence;
using OnlineCourses.Application.Enrollments.Services;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// JWT Auth (config mínima)
var jwtKey = builder.Configuration["Jwt:Key"] ?? "DEV_SECRET_KEY_CHANGE";
var keyBytes = Encoding.UTF8.GetBytes(jwtKey);
builder.Services.AddAuthentication(o =>
{
    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ClockSkew = TimeSpan.Zero
    };
});

// Dependency Injection
builder.Services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase("OnlineCoursesDb"));
builder.Services.AddScoped<ICourseRepository, EfCourseRepository>();
builder.Services.AddScoped<IEnrollmentRepository, EfEnrollmentRepository>();
builder.Services.AddScoped<CourseService>();
builder.Services.AddScoped<EnrollmentService>();
builder.Services.AddSingleton<AuthService>(); // InMemory store

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/api/courses", async (CourseService service, string? category, string? level, string? q, CancellationToken ct) =>
{
    var result = await service.GetCoursesAsync(category, level, q, ct);
    return Results.Ok(result);
});

app.MapPost("/api/auth/register", (AuthService auth, RegisterRequest req) =>
{
    try { return Results.Ok(auth.Register(req)); }
    catch (Exception ex) { return Results.BadRequest(new { error = ex.Message }); }
});

app.MapPost("/api/auth/login", (AuthService auth, LoginRequest req) =>
{
    try { return Results.Ok(auth.Login(req)); }
    catch (Exception) { return Results.Unauthorized(); }
});

app.MapPost("/api/enrollments", async (EnrollmentService service, Guid courseId, ClaimsPrincipal user, CancellationToken ct) =>
{
    if (!Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)) return Results.Unauthorized();
    try { var dto = await service.EnrollAsync(courseId, userId, ct); return Results.Ok(dto); }
    catch (Exception ex) { return Results.BadRequest(new { error = ex.Message }); }
}).RequireAuthorization();

app.MapGet("/api/enrollments/my", async (EnrollmentService service, ClaimsPrincipal user, CancellationToken ct) =>
{
    if (!Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)) return Results.Unauthorized();
    var list = await service.GetMyAsync(userId, ct);
    return Results.Ok(list);
}).RequireAuthorization();

app.Run();