using Microsoft.EntityFrameworkCore;
using OnlineCourses.Application.Enrollments;
using OnlineCourses.Domain.Entities;
using OnlineCourses.Infrastructure.Persistence;

namespace OnlineCourses.Infrastructure.Enrollments;

public class EfEnrollmentRepository : IEnrollmentRepository
{
    private readonly OnlineCoursesDbContext _db;
    public EfEnrollmentRepository(OnlineCoursesDbContext db) => _db = db;

    public async Task AddAsync(Enrollment enrollment, CancellationToken ct = default)
    {
        _db.Enrollments.Add(enrollment);
        await _db.SaveChangesAsync(ct);
    }

    public Task<bool> ExistsAsync(Guid courseId, Guid userId, CancellationToken ct = default)
        => _db.Enrollments.AnyAsync(e => e.CourseId == courseId && e.UserId == userId, ct);

    public async Task<IEnumerable<Enrollment>> GetByUserAsync(Guid userId, CancellationToken ct = default)
        => await _db.Enrollments.AsNoTracking().Where(e => e.UserId == userId).ToListAsync(ct);
}