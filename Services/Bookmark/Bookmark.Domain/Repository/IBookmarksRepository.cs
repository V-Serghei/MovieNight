using Bookmark.Domain.Entity;

namespace Bookmark.Domain.Repository;

public interface IBookmarksRepository
{
    Task<IReadOnlyList<Entity.Bookmark>> GetByUserAsync(
        Guid userId,
        BookmarkKind? kind,
        CancellationToken ct);

    Task<Entity.Bookmark?> FindAsync(
        Guid userId,
        Guid movieId,
        BookmarkKind kind,
        CancellationToken ct);

    Task AddAsync(Entity.Bookmark bookmark, CancellationToken ct);
    Task RemoveAsync(Entity.Bookmark bookmark, CancellationToken ct);

    Task RemoveByMovieAsync(Guid userId, Guid movieId, CancellationToken ct);

    Task RemoveExpiredTemporaryAsync(DateTimeOffset now, CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);
}