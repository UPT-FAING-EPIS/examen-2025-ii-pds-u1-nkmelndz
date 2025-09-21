using OnlineCourses.Domain.Entities;

namespace OnlineCourses.Domain.Repositories;

public interface ICourseRepository
{
    Task<IEnumerable<Course>> GetAllAsync(string? category = null, string? level = null, string? search = null, CancellationToken ct = default);
    Task<Course?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Course course, CancellationToken ct = default);
    Task UpdateAsync(Course course, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}