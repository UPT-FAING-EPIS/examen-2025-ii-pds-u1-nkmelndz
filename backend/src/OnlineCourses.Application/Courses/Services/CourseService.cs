using OnlineCourses.Application.Courses.DTOs;
using OnlineCourses.Domain.Repositories;

namespace OnlineCourses.Application.Courses.Services;

public class CourseService
{
    private readonly ICourseRepository _repository;

    public CourseService(ICourseRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<CourseDto>> GetCoursesAsync(string? category, string? level, string? search, CancellationToken ct = default)
    {
        var courses = await _repository.GetAllAsync(category, level, search, ct);
        return courses.Select(c => new CourseDto(c.Id, c.Title, c.Description, c.Category, c.Level, c.DurationHours, c.Price, c.IsPublished));
    }
}