using Microsoft.EntityFrameworkCore;
using Users.Core.Entities;
using Users.Core.Repositories;
using Users.Infrastructure.Data;

namespace Users.Infrastructure.Configurations;

public class UserRepository(UsersDbContext db) : IUserRepository
{
    public Task<bool> ExistsByEmailAsync(string emailNorm, CancellationToken ct = default)
        => db.Users.AnyAsync(u => u.EmailNormalized == emailNorm, ct);


    public Task<User?> FindByEmailAsync(string emailNorm, CancellationToken ct = default)
        => db.Users.SingleOrDefaultAsync(u => u.EmailNormalized == emailNorm, ct);


    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.Users.SingleOrDefaultAsync(u => u.Id == id, ct);

    public Task<List<User>?> GetAllAsync(CancellationToken ct = default)
    {
        return db.Users.ToListAsync(ct)!;
    }


    public Task AddAsync(User user, CancellationToken ct = default)
        => db.Users.AddAsync(user, ct).AsTask();


    public Task SaveChangesAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);
}