using Microsoft.EntityFrameworkCore;
using OnlineCourses.Domain.Entities;
using OnlineCourses.Domain.Repositories;

namespace OnlineCourses.Infrastructure.Persistence;

public class EfEnrollmentRepository : IEnrollmentRepository
{
    private readonly AppDbContext _db;
    public EfEnrollmentRepository(AppDbContext db) => _db = db;

    public async Task AddAsync(Enrollment enrollment, CancellationToken ct = default)
    {
        await _db.Enrollments.AddAsync(enrollment, ct);
        await _db.SaveChangesAsync(ct);
    }

    public Task<bool> ExistsAsync(Guid courseId, Guid userId, CancellationToken ct = default)
        => _db.Enrollments.AnyAsync(e => e.CourseId == courseId && e.UserId == userId, ct);

    public Task<Enrollment?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Enrollments.Include(e => e.Course).FirstOrDefaultAsync(e => e.Id == id, ct)!;

    public async Task<IEnumerable<Enrollment>> GetByUserAsync(Guid userId, CancellationToken ct = default)
        => await _db.Enrollments.Include(e => e.Course).Where(e => e.UserId == userId).ToListAsync(ct);
}