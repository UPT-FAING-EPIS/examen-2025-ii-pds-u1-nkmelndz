using OnlineCourses.Domain.Entities;

namespace OnlineCourses.Application.Enrollments;

public interface IEnrollmentRepository
{
    Task AddAsync(Enrollment enrollment, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid courseId, Guid userId, CancellationToken ct = default);
    Task<IEnumerable<Enrollment>> GetByUserAsync(Guid userId, CancellationToken ct = default);
}