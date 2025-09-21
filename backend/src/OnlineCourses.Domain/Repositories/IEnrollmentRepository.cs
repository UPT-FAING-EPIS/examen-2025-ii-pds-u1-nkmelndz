using OnlineCourses.Domain.Entities;

namespace OnlineCourses.Domain.Repositories;

public interface IEnrollmentRepository
{
    Task<Enrollment?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<Enrollment>> GetByUserAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(Enrollment enrollment, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid courseId, Guid userId, CancellationToken ct = default);
}