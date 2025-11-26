using Friends.Domain.Repository;
using Friends.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Friends.Infrastructure.Repositories;

public class FriendsRepository (FriendsDbContext db) : IFriendsRepository
{
    public async Task<List<Domain.Entities.Friends>> FindFriendsByUserIdAsync(
        string userId)
    {
        return await db.Friends
            .Where(f => f.IdUser == userId)
            .ToListAsync();
    }

    public Task AddAsync(
        Domain.Entities.Friends friendship,
        CancellationToken ct = default)
    {
        return db.Friends.AddAsync(friendship, ct).AsTask();
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
    {
        return db.SaveChangesAsync(ct);
    }
}