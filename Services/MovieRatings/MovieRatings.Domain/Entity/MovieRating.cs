namespace MovieRatings.Domain.Entity;

public class MovieRatings
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public Guid MovieId { get; set; }

    public int Score { get; set; } // 1..10

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}