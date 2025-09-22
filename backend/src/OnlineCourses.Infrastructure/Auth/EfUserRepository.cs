using Microsoft.EntityFrameworkCore;
using OnlineCourses.Application.Auth;
using OnlineCourses.Domain.Entities;
using OnlineCourses.Infrastructure.Persistence;

namespace OnlineCourses.Infrastructure.Auth;

public class EfUserRepository : IUserRepository
{
    private readonly OnlineCoursesDbContext _db;
    public EfUserRepository(OnlineCoursesDbContext db) => _db = db;

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email, ct)!;

    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, ct)!;

    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);
    }
}