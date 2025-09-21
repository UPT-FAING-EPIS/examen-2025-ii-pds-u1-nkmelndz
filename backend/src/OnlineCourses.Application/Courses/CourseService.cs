using OnlineCourses.Domain.Entities;
using OnlineCourses.Application.Courses.Dtos;

namespace OnlineCourses.Application.Courses;

public class CourseService
{
    private readonly ICourseRepository _repo;

    public CourseService(ICourseRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<CourseListItemDto>> ListAsync(string? category, string? level, string? q, CancellationToken ct = default)
    {
        var courses = await _repo.GetAllAsync(category, level, q, ct);
        return courses.Select(c => new CourseListItemDto(c.Id, c.Title, c.Category, c.Level, c.IsPublished, c.Price));
    }

    public async Task<CourseDetailDto?> GetAsync(Guid id, CancellationToken ct = default)
    {
        var c = await _repo.GetByIdAsync(id, ct);
        return c == null ? null : new CourseDetailDto(c.Id, c.Title, c.Description, c.Category, c.Level, c.Syllabus, c.DurationHours, c.Price, c.IsPublished, c.InstructorId, c.CreatedAt);
    }

    public async Task<Guid> CreateAsync(CreateCourseRequest req, Guid instructorId, CancellationToken ct = default)
    {
        var course = new Course
        {
            Title = req.Title,
            Description = req.Description,
            Category = req.Category,
            Level = req.Level,
            Syllabus = req.Syllabus,
            DurationHours = req.DurationHours,
            Price = req.Price,
            IsPublished = req.IsPublished,
            InstructorId = instructorId
        };
        await _repo.AddAsync(course, ct);
        return course.Id;
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateCourseRequest req, Guid instructorId, bool isAdmin, CancellationToken ct = default)
    {
        var existing = await _repo.GetByIdAsync(id, ct);
        if (existing == null) return false;
        if (!isAdmin && existing.InstructorId != instructorId) throw new UnauthorizedAccessException("No puede editar este curso");
        existing.Title = req.Title;
        existing.Description = req.Description;
        existing.Category = req.Category;
        existing.Level = req.Level;
        existing.Syllabus = req.Syllabus;
        existing.DurationHours = req.DurationHours;
        existing.Price = req.Price;
        existing.IsPublished = req.IsPublished;
        await _repo.UpdateAsync(existing, ct);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, bool isAdmin, CancellationToken ct = default)
    {
        if (!isAdmin) throw new UnauthorizedAccessException("Solo admin puede eliminar");
        var existing = await _repo.GetByIdAsync(id, ct);
        if (existing == null) return false;
        await _repo.DeleteAsync(existing, ct);
        return true;
    }
}