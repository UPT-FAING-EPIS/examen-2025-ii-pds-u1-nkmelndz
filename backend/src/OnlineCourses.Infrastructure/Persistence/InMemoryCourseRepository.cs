using OnlineCourses.Domain.Entities;
using OnlineCourses.Domain.Repositories;

namespace OnlineCourses.Infrastructure.Persistence;

public class InMemoryCourseRepository : ICourseRepository
{
    private readonly List<Course> _courses = new();

    public Task AddAsync(Course course, CancellationToken ct = default)
    {
        _courses.Add(course);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var c = _courses.FirstOrDefault(x => x.Id == id);
        if (c != null) _courses.Remove(c);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<Course>> GetAllAsync(string? category = null, string? level = null, string? search = null, CancellationToken ct = default)
    {
        IEnumerable<Course> query = _courses;
        if (!string.IsNullOrWhiteSpace(category)) query = query.Where(c => c.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(level)) query = query.Where(c => c.Level.Equals(level, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(c => c.Title.Contains(search, StringComparison.OrdinalIgnoreCase) || c.Description.Contains(search, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(query);
    }

    public Task<Course?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return Task.FromResult(_courses.FirstOrDefault(c => c.Id == id));
    }

    public Task UpdateAsync(Course course, CancellationToken ct = default)
    {
        var existing = _courses.FindIndex(c => c.Id == course.Id);
        if (existing >= 0)
        {
            _courses[existing] = course;
        }
        return Task.CompletedTask;
    }
}