namespace Friends.Domain.Repository;

public interface IFriendsRepository
{
    Task<List<Entities.Friends>> FindFriendsByUserIdAsync(string userId);
    Task AddAsync(Entities.Friends messages, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}