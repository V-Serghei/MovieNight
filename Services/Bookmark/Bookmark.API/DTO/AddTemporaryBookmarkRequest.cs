namespace Bookmark.API.DTO;

public record AddTemporaryBookmarkRequest(
    UIMovieDto Movie,
    int? TtlMinutes
);