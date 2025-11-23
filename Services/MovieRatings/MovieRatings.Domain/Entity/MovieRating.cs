namespace MovieRatings.Domain.Entity;

public class MovieRating
{
    public Guid UserId { get; set; }
    public Guid MovieId { get; set; }

    public int Score { get; set; } // 1..10
    public DateTime RatedAt { get; set; }
}