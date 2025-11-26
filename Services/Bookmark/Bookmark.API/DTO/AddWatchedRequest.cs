namespace Bookmark.API.DTO;

public record AddWatchedRequest(
    UIMovieDto Movie,
    DateTimeOffset? WatchedAt
);