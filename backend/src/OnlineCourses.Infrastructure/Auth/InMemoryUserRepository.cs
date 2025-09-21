using OnlineCourses.Application.Auth;
using OnlineCourses.Domain.Entities;

namespace OnlineCourses.Infrastructure.Auth;

public class InMemoryUserRepository : IUserRepository
{
    private readonly List<User> _users = new();

    public Task AddAsync(User user, CancellationToken ct = default)
    {
        _users.Add(user); return Task.CompletedTask;
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => Task.FromResult(_users.SingleOrDefault(u => u.Email == email));

    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(_users.SingleOrDefault(u => u.Id == id));
}