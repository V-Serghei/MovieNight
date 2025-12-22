using Bookmark.Domain.Entity;

namespace Bookmark.API.DTO;

public record BookmarkResponse(
    Guid Id,
    Guid MovieId,
    BookmarkKind Kind,
    DateTimeOffset CreatedAt,
    DateTimeOffset? WatchedAt,
    DateTimeOffset? ExpiresAt,
    UIMovieDto Movie
);