using OnlineCourses.Application.Courses;
using OnlineCourses.Application.Auth;
using OnlineCourses.Application.Auth.Dtos;
using OnlineCourses.Application.Enrollments;
using OnlineCourses.Application.Enrollments.Dtos;
using OnlineCourses.Infrastructure.Persistence;
using OnlineCourses.Infrastructure.Auth;
using OnlineCourses.Infrastructure.Enrollments;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Configuración dinámica de URLs: primero App:Urls en config, luego variable APP_URLS / ASPNETCORE_URLS.
// Si nada se especifica, Kestrel elegirá los defaults (http://localhost:5000 y https://localhost:7000 si hay cert dev).
var configuredUrls = builder.Configuration["App:Urls"]
    ?? Environment.GetEnvironmentVariable("APP_URLS")
    ?? Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
if (!string.IsNullOrWhiteSpace(configuredUrls))
{
    builder.WebHost.UseUrls(configuredUrls);
    Console.WriteLine($"[HOST] Usando URLs configuradas: {configuredUrls}");
}
else
{
    Console.WriteLine("[HOST] No se configuró App:Urls / APP_URLS. Intentando fallback IPv4 puerto dinámico...");
    // Fallback: forzar IPv4 loopback con puerto dinámico si el binding estándar (localhost IPv6) está bloqueado.
    builder.WebHost.ConfigureKestrel(k =>
    {
        k.Listen(IPAddress.Loopback, 0); // 0 => puerto dinámico asignado por el SO
    });
}

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext PostgreSQL (log de cadena de conexión enmascarada para diagnóstico)
string rawCs = builder.Configuration.GetConnectionString("Default")
    ?? "Host=localhost;Port=5433;Database=onlinecourses_dev;Username=online;Password=onlinepwd"; // fallback actualizado a 5433
string Mask(string cs) => Regex.Replace(cs, "(?i)(password=)([^;]+)", "$1****");
Console.WriteLine($"[DB] ConnectionString (effective): {Mask(rawCs)}");
builder.Services.AddDbContext<OnlineCoursesDbContext>(options => options.UseNpgsql(rawCs));

// Repositorios EF
builder.Services.AddScoped<ICourseRepository, EfCourseRepository>();
builder.Services.AddScoped<IUserRepository, EfUserRepository>();
builder.Services.AddScoped<IEnrollmentRepository, EfEnrollmentRepository>();
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

// Log de direcciones efectivas al iniciar (puerto dinámico incluido)
app.Lifetime.ApplicationStarted.Register(() =>
{
    Console.WriteLine("[HOST] Direcciones efectivas levantadas:");
    foreach (var u in app.Urls)
        Console.WriteLine("  " + u);
    Console.WriteLine("[HOST] Abra /swagger para la UI.");
});

// Migraciones y seeding (solo desarrollo / arranque)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OnlineCoursesDbContext>();
    try
    {
        db.Database.Migrate();
        if (!db.Users.Any())
        {
            // Hash simple temporal (igual que AuthService) -> luego reemplazar por BCrypt
            static string Hash(string p) => Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(p)));
            var admin = new OnlineCourses.Domain.Entities.User { Email = "admin@demo.com", FullName = "Admin Demo", PasswordHash = Hash("Admin123!"), Role = OnlineCourses.Domain.Entities.UserRole.Admin };
            var instructor = new OnlineCourses.Domain.Entities.User { Email = "instructor@demo.com", FullName = "Instructor Demo", PasswordHash = Hash("Instructor123!"), Role = OnlineCourses.Domain.Entities.UserRole.Instructor };
            db.Users.AddRange(admin, instructor);
            db.Courses.Add(new OnlineCourses.Domain.Entities.Course
            {
                Title = "Curso Intro",
                Description = "Curso de ejemplo seed",
                Category = "general",
                Level = "beginner",
                Syllabus = "Temario inicial",
                DurationHours = 5,
                Price = 0,
                InstructorId = instructor.Id,
                IsPublished = true
            });
            db.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[SEED ERROR] {ex.Message}");
    }
}

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