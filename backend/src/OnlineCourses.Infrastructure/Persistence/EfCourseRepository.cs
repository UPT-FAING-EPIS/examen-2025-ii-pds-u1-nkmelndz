using Microsoft.EntityFrameworkCore;
using OnlineCourses.Domain.Entities;
using OnlineCourses.Domain.Repositories;

namespace OnlineCourses.Infrastructure.Persistence;

public class EfCourseRepository : ICourseRepository
{
    private readonly AppDbContext _db;
    public EfCourseRepository(AppDbContext db) => _db = db;

    public async Task AddAsync(Course course, CancellationToken ct = default)
    {
        await _db.Courses.AddAsync(course, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.Courses.FindAsync(new object?[] { id }, ct);
        if (entity != null)
        {
            _db.Courses.Remove(entity);
            await _db.SaveChangesAsync(ct);
        }
    }

    public async Task<IEnumerable<Course>> GetAllAsync(string? category = null, string? level = null, string? search = null, CancellationToken ct = default)
    {
        IQueryable<Course> query = _db.Courses.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(category)) query = query.Where(c => c.Category == category);
        if (!string.IsNullOrWhiteSpace(level)) query = query.Where(c => c.Level == level);
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(c => c.Title.Contains(search) || c.Description.Contains(search));
        return await query.ToListAsync(ct);
    }

    public Task<Course?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Courses.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct)!;

    public async Task UpdateAsync(Course course, CancellationToken ct = default)
    {
        _db.Courses.Update(course);
        await _db.SaveChangesAsync(ct);
    }
}