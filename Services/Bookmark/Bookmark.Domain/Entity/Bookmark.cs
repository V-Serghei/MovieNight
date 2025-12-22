namespace Bookmark.Domain.Entity;

public class Bookmark
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public Guid MovieId { get; set; }

    public BookmarkKind Kind { get; set; }
    public string MovieTitle { get; set; } = "";
    public int MovieYear { get; set; }
    public string? MovieDuration { get; set; }
    public string? MoviePosterImage { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? WatchedAt { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
}