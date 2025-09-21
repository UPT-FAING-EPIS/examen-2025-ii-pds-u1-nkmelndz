using OnlineCourses.Application.Courses;
using OnlineCourses.Domain.Entities;

namespace OnlineCourses.Infrastructure.Persistence;

public class InMemoryCourseRepository : ICourseRepository
{
    private readonly List<Course> _courses = new();

    public Task AddAsync(Course course, CancellationToken ct = default)
    {
        _courses.Add(course);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Course course, CancellationToken ct = default)
    {
        _courses.Remove(course);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(_courses.Any(c => c.Id == id));

    public Task<IEnumerable<Course>> GetAllAsync(string? category, string? level, string? q, CancellationToken ct = default)
    {
        IEnumerable<Course> query = _courses;
        if (!string.IsNullOrWhiteSpace(category)) query = query.Where(c => c.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(level)) query = query.Where(c => c.Level.Equals(level, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(c => c.Title.Contains(q, StringComparison.OrdinalIgnoreCase) || c.Description.Contains(q, StringComparison.OrdinalIgnoreCase));
        // OrderBy devuelve IOrderedEnumerable<Course>; explicitamente lo exponemos como IEnumerable para que coincida la firma
        var ordered = query.OrderBy(c => c.Title).AsEnumerable();
        return Task.FromResult<IEnumerable<Course>>(ordered);
    }

    public Task<Course?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(_courses.SingleOrDefault(c => c.Id == id));

    public Task UpdateAsync(Course course, CancellationToken ct = default)
    {
        // nada adicional; ya está referenciado en la lista
        return Task.CompletedTask;
    }
}