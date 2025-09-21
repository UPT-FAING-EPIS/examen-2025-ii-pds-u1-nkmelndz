using OnlineCourses.Domain.Entities;

namespace OnlineCourses.Application.Courses;

public interface ICourseRepository
{
    Task<IEnumerable<Course>> GetAllAsync(string? category, string? level, string? q, CancellationToken ct = default);
    Task<Course?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Course course, CancellationToken ct = default);
    Task UpdateAsync(Course course, CancellationToken ct = default);
    Task DeleteAsync(Course course, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
}