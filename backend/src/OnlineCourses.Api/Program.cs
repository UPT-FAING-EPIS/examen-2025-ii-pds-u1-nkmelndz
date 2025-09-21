using OnlineCourses.Application.Courses;
using OnlineCourses.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<ICourseRepository, InMemoryCourseRepository>();
builder.Services.AddScoped<CourseService>();

var app = builder.Build();

// Swagger habilitado siempre mientras se desarrolla (luego condicionarlo por entorno)
app.UseSwagger();
app.UseSwaggerUI();

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

app.Run();