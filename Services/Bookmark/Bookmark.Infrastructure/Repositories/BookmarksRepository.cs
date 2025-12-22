using Bookmark.Domain.Entity;
using Bookmark.Domain.Repository;
using Bookmark.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Bookmark.Infrastructure.Repositories;

public class BookmarksRepository : IBookmarksRepository
{
    private readonly BookmarksDbContext _db;

    public BookmarksRepository(BookmarksDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<Domain.Entity.Bookmark>> GetByUserAsync(
        Guid userId,
        BookmarkKind? kind,
        CancellationToken ct)
    {
        var query = _db.Bookmarks.AsQueryable()
            .Where(x => x.UserId == userId);

        if (kind.HasValue)
            query = query.Where(x => x.Kind == kind.Value);

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);
    }

    public Task<Domain.Entity.Bookmark?> FindAsync(
        Guid userId,
        Guid movieId,
        BookmarkKind kind,
        CancellationToken ct)
    {
        return _db.Bookmarks
            .FirstOrDefaultAsync(x => x.UserId == userId &&
                                      x.MovieId == movieId &&
                                      x.Kind == kind, ct);
    }

    public async Task AddAsync(Domain.Entity.Bookmark bookmark, CancellationToken ct)
    {
        await _db.Bookmarks.AddAsync(bookmark, ct);
    }

    public Task RemoveAsync(Domain.Entity.Bookmark bookmark, CancellationToken ct)
    {
        _db.Bookmarks.Remove(bookmark);
        return Task.CompletedTask;
    }

    public async Task RemoveByMovieAsync(Guid userId, Guid movieId, CancellationToken ct)
    {
        var items = await _db.Bookmarks
            .Where(x => x.UserId == userId && x.MovieId == movieId)
            .ToListAsync(ct);

        _db.Bookmarks.RemoveRange(items);
    }

    public async Task RemoveExpiredTemporaryAsync(DateTimeOffset now, CancellationToken ct)
    {
        var expired = await _db.Bookmarks
            .Where(x =>
                x.Kind == BookmarkKind.Temporary &&
                x.ExpiresAt != null &&
                x.ExpiresAt <= now)
            .ToListAsync(ct);

        if (expired.Count > 0)
            _db.Bookmarks.RemoveRange(expired);
    }

    public Task SaveChangesAsync(CancellationToken ct)
        => _db.SaveChangesAsync(ct);
}