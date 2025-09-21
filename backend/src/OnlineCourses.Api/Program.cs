using OnlineCourses.Application.Courses;
using OnlineCourses.Application.Auth;
using OnlineCourses.Application.Auth.Dtos;
using OnlineCourses.Application.Enrollments;
using OnlineCourses.Application.Enrollments.Dtos;
using OnlineCourses.Infrastructure.Persistence;
using OnlineCourses.Infrastructure.Auth;
using OnlineCourses.Infrastructure.Enrollments;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<ICourseRepository, InMemoryCourseRepository>();
builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();
builder.Services.AddSingleton<IEnrollmentRepository, InMemoryEnrollmentRepository>();
builder.Services.AddSingleton<ITokenGenerator, SimpleJwtGenerator>();
builder.Services.AddScoped<CourseService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<EnrollmentService>();

// JWT Auth (config mínima)
var jwtKey = builder.Configuration["Jwt:Key"] ?? "dev-secret-unsafe";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "OnlineCourses";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "OnlineCoursesAudience";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Swagger habilitado siempre mientras se desarrolla (luego condicionarlo por entorno)
app.UseSwagger();
app.UseSwaggerUI();

// Middlewares de seguridad antes de los endpoints
app.UseAuthentication();
app.UseAuthorization();

// Redirect conveniente /swagger -> /swagger/index.html
app.MapGet("/swagger", () => Results.Redirect("/swagger/index.html"));

app.MapGet("/api/courses", async (string? category, string? level, string? q, CourseService service, CancellationToken ct) =>
{
    var list = await service.ListAsync(category, level, q, ct);
    return Results.Ok(list);
});

app.MapGet("/api/courses/{id:guid}", async (Guid id, CourseService service, CancellationToken ct) =>
{
    var course = await service.GetAsync(id, ct);
    return course is null ? Results.NotFound() : Results.Ok(course);
});

// Simulación de autenticación (placeholder) – instructorId y roles hardcodeados para ahora
app.MapPost("/api/courses", async (OnlineCourses.Application.Courses.Dtos.CreateCourseRequest req, CourseService service, CancellationToken ct) =>
{
    var fakeInstructorId = Guid.Parse("00000000-0000-0000-0000-000000000111");
    var id = await service.CreateAsync(req, fakeInstructorId, ct);
    return Results.Created($"/api/courses/{id}", new { id });
});

app.MapPut("/api/courses/{id:guid}", async (Guid id, OnlineCourses.Application.Courses.Dtos.UpdateCourseRequest req, CourseService service, CancellationToken ct) =>
{
    var fakeInstructorId = Guid.Parse("00000000-0000-0000-0000-000000000111");
    bool isAdmin = false; // placeholder
    try
    {
        var updated = await service.UpdateAsync(id, req, fakeInstructorId, isAdmin, ct);
        return updated ? Results.NoContent() : Results.NotFound();
    }
    catch (UnauthorizedAccessException)
    {
        return Results.StatusCode(StatusCodes.Status403Forbidden);
    }
});

app.MapDelete("/api/courses/{id:guid}", async (Guid id, CourseService service, CancellationToken ct) =>
{
    bool isAdmin = true; // placeholder para permitir borrado
    try
    {
        var deleted = await service.DeleteAsync(id, isAdmin, ct);
        return deleted ? Results.NoContent() : Results.NotFound();
    }
    catch (UnauthorizedAccessException)
    {
        return Results.StatusCode(StatusCodes.Status403Forbidden);
    }
});

// AUTH
app.MapPost("/api/auth/register", async (RegisterRequest req, AuthService auth, CancellationToken ct) =>
{
    try
    {
        var resp = await auth.RegisterAsync(req, ct);
        return Results.Created($"/api/users/{resp.UserId}", resp);
    }
    catch (InvalidOperationException ex)
    {
        return Results.Conflict(new { message = ex.Message });
    }
    catch (ArgumentNullException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
    catch (ArgumentException ex) // Incluye ArgumentNullException si llegara después, pero ya capturado arriba
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

app.MapPost("/api/auth/login", async (LoginRequest req, AuthService auth, CancellationToken ct) =>
{
    try
    {
        var resp = await auth.LoginAsync(req, ct);
        return Results.Ok(resp);
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
});

// ENROLLMENTS (requiere autenticación)
app.MapPost("/api/enrollments", async (EnrollRequest req, EnrollmentService service, HttpContext http, CancellationToken ct) =>
{
    if (!http.User.Identity?.IsAuthenticated ?? true)
        return Results.Unauthorized();
    var userId = Guid.Parse(http.User.FindFirst("sub")!.Value);
    try
    {
        var id = await service.EnrollAsync(userId, req, ct);
        return Results.Created($"/api/enrollments/{id}", new { id });
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(new { message = ex.Message });
    }
    catch (InvalidOperationException ex)
    {
        return Results.Conflict(new { message = ex.Message });
    }
});

app.MapGet("/api/enrollments/my", async (EnrollmentService service, HttpContext http, CancellationToken ct) =>
{
    if (!http.User.Identity?.IsAuthenticated ?? true)
        return Results.Unauthorized();
    var userId = Guid.Parse(http.User.FindFirst("sub")!.Value);
    var list = await service.ListMineAsync(userId, ct);
    return Results.Ok(list);
});

app.Run();