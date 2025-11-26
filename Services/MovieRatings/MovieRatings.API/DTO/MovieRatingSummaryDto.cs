namespace MovieRatings.API.DTO;

public record MovieRatingSummaryDto(
    Guid MovieId,
    double? AverageRating,
    int RatingsCount,
    int? UserRating
);