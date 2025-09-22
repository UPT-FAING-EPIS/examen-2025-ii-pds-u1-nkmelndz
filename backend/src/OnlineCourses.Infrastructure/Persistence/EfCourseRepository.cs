using Microsoft.EntityFrameworkCore;
using OnlineCourses.Application.Courses;
using OnlineCourses.Domain.Entities;

namespace OnlineCourses.Infrastructure.Persistence;

public class EfCourseRepository : ICourseRepository
{
    private readonly OnlineCoursesDbContext _db;
    public EfCourseRepository(OnlineCoursesDbContext db) => _db = db;

    public async Task<IEnumerable<Course>> GetAllAsync(string? category, string? level, string? q, CancellationToken ct = default)
    {
        IQueryable<Course> query = _db.Courses.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(category)) query = query.Where(c => c.Category == category);
        if (!string.IsNullOrWhiteSpace(level)) query = query.Where(c => c.Level == level);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(c => c.Title.ToLower().Contains(term) || c.Description.ToLower().Contains(term));
        }
        return await query.OrderByDescending(c => c.CreatedAt).ToListAsync(ct);
    }

    public Task<Course?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Courses.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct)!;

    public async Task AddAsync(Course course, CancellationToken ct = default)
    {
        _db.Courses.Add(course);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Course course, CancellationToken ct = default)
    {
        _db.Courses.Update(course);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Course course, CancellationToken ct = default)
    {
        _db.Courses.Remove(course);
        await _db.SaveChangesAsync(ct);
    }

    public Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
        => _db.Courses.AnyAsync(c => c.Id == id, ct);
}