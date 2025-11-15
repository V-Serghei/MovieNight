using Microsoft.EntityFrameworkCore;
using Users.Core.Entities;
using Users.Core.Repositories;
using Users.Infrastructure.Data;

namespace Users.Infrastructure.Configurations;

public class UserProfileRepository : IUserProfileRepository
{
    private readonly UsersDbContext _db;

    public UserProfileRepository(UsersDbContext db)
    {
        _db = db;
    }

    public Task<UserProfile?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        => _db.UserProfiles.AsNoTracking().SingleOrDefaultAsync(p => p.UserId == userId, ct);

    public async Task AddOrUpdateAsync(UserProfile profile, CancellationToken ct = default)
    {
        var existing = await _db.UserProfiles
            .SingleOrDefaultAsync(p => p.UserId == profile.UserId, ct);

        if (existing is null)
        {
            await _db.UserProfiles.AddAsync(profile, ct);
        }
        else
        {
            _db.Entry(existing).CurrentValues.SetValues(profile);
        }
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}