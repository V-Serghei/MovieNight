using MovieRatings.Domain.Entity;

namespace MovieRatings.Domain.Repository;

public interface IRatingsRepository
{
    Task<Entity.MovieRatings?> GetUserRatingAsync(Guid userId, Guid movieId, CancellationToken ct);
    Task<MovieRatingAggregate?> GetAggregateAsync(Guid movieId, CancellationToken ct);

    Task<(Entity.MovieRatings rating, MovieRatingAggregate aggregate)> SetRatingAsync(
        Guid userId,
        Guid movieId,
        int score,
        CancellationToken ct);

    Task<MovieRatingAggregate?> DeleteRatingAsync(
        Guid userId,
        Guid movieId,
        CancellationToken ct);

    Task<(IReadOnlyList<MovieRatingAggregate> aggregates,
            IReadOnlyList<Entity.MovieRatings> userRatings)>
        GetSummariesAsync(Guid? userId, IReadOnlyList<Guid> movieIds, CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);
}