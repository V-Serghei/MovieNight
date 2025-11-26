namespace MovieRatings.Domain.Entity;

public class MovieRatingAggregate
{
    public Guid MovieId { get; set; }

    public int RatingsCount { get; set; }
    public int RatingsSum { get; set; }

    public double AverageScore { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}