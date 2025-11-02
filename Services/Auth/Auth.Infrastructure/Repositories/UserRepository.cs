using Auth.Domain.Entities;
using Auth.Domain.Repository.User;
using Auth.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Repositories;

public class UserRepository(AuthDbContext db) : IUserRepository
{
    public Task<User?> FindByEmailAsync(string email, CancellationToken ct = default)
    {
        return db.Users.SingleOrDefaultAsync(u => u.Email == email, ct);
    }

    public Task AddAsync(User user, CancellationToken ct = default)
    {
        return db.Users.AddAsync(user, ct).AsTask();
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
    {
        return db.SaveChangesAsync(ct);
    }
}