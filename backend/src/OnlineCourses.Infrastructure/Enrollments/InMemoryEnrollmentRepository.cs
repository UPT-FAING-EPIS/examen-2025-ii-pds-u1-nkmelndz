using OnlineCourses.Application.Enrollments;
using OnlineCourses.Domain.Entities;

namespace OnlineCourses.Infrastructure.Enrollments;

public class InMemoryEnrollmentRepository : IEnrollmentRepository
{
    private readonly List<Enrollment> _enrollments = new();

    public Task AddAsync(Enrollment enrollment, CancellationToken ct = default)
    {
        _enrollments.Add(enrollment); return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(Guid courseId, Guid userId, CancellationToken ct = default)
        => Task.FromResult(_enrollments.Any(e => e.CourseId == courseId && e.UserId == userId));

    public Task<IEnumerable<Enrollment>> GetByUserAsync(Guid userId, CancellationToken ct = default)
        => Task.FromResult(_enrollments.Where(e => e.UserId == userId).AsEnumerable());
}