namespace Bookmark.Domain.Entity;

public class UserMovieState
{
    public Guid UserId { get; set; }
    public Guid MovieId { get; set; }

    public bool IsBookmarked { get; set; }

    public bool IsTemporary { get; set; }
    public DateTime? TemporaryExpiresAt { get; set; }

    public bool IsWatched { get; set; }
    public DateTime? WatchedAt { get; set; }
}