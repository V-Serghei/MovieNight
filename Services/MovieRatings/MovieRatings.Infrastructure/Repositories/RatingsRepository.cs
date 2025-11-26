using Microsoft.EntityFrameworkCore;
using MovieRatings.Domain.Entity;
using MovieRatings.Domain.Repository;
using MovieRatings.Infrastructure.Data;

namespace MovieRatings.Infrastructure.Repositories;

public class RatingsRepository : IRatingsRepository
{
    private readonly RatingsDbContext _db;

    public RatingsRepository(RatingsDbContext db)
    {
        _db = db;
    }

    public Task<Domain.Entity.MovieRatings?> GetUserRatingAsync(Guid userId, Guid movieId, CancellationToken ct)
    {
        return _db.Ratings.FirstOrDefaultAsync(
            x => x.UserId == userId && x.MovieId == movieId, ct);
    }

    public Task<MovieRatingAggregate?> GetAggregateAsync(Guid movieId, CancellationToken ct)
    {
        return _db.MovieRatingAggregates.FirstOrDefaultAsync(
            x => x.MovieId == movieId, ct);
    }

    public async Task<(Domain.Entity.MovieRatings rating, MovieRatingAggregate aggregate)> SetRatingAsync(
        Guid userId,
        Guid movieId,
        int score,
        CancellationToken ct)
    {
        if (score < 1 || score > 10)
            throw new ArgumentOutOfRangeException(nameof(score), "Score must be between 1 and 10.");

        var now = DateTimeOffset.UtcNow;

        using var tx = await _db.Database.BeginTransactionAsync(ct);

        var rating = await _db.Ratings
            .FirstOrDefaultAsync(x => x.UserId == userId && x.MovieId == movieId, ct);

        MovieRatingAggregate? agg = await _db.MovieRatingAggregates
            .FirstOrDefaultAsync(x => x.MovieId == movieId, ct);

        if (rating is null)
        {
            rating = new Domain.Entity.MovieRatings
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                MovieId = movieId,
                Score = score,
                CreatedAt = now,
                UpdatedAt = now
            };
            await _db.Ratings.AddAsync(rating, ct);

            if (agg is null)
            {
                agg = new MovieRatingAggregate
                {
                    MovieId = movieId,
                    RatingsCount = 1,
                    RatingsSum = score,
                    AverageScore = score,
                    UpdatedAt = now
                };
                await _db.MovieRatingAggregates.AddAsync(agg, ct);
            }
            else
            {
                agg.RatingsCount += 1;
                agg.RatingsSum += score;
                agg.AverageScore = (double)agg.RatingsSum / agg.RatingsCount;
                agg.UpdatedAt = now;
            }
        }
        else
        {
            var oldScore = rating.Score;
            if (oldScore != score)
            {
                rating.Score = score;
                rating.UpdatedAt = now;

                if (agg is null)
                {
                    agg = new MovieRatingAggregate
                    {
                        MovieId = movieId,
                        RatingsCount = 1,
                        RatingsSum = score,
                        AverageScore = score,
                        UpdatedAt = now
                    };
                    await _db.MovieRatingAggregates.AddAsync(agg, ct);
                }
                else
                {
                    agg.RatingsSum = agg.RatingsSum - oldScore + score;
                    if (agg.RatingsCount <= 0)
                        agg.RatingsCount = 1;

                    agg.AverageScore = (double)agg.RatingsSum / agg.RatingsCount;
                    agg.UpdatedAt = now;
                }
            }
        }

        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return (rating, agg!);
    }

    public async Task<MovieRatingAggregate?> DeleteRatingAsync(
        Guid userId,
        Guid movieId,
        CancellationToken ct)
    {
        using var tx = await _db.Database.BeginTransactionAsync(ct);

        var rating = await _db.Ratings
            .FirstOrDefaultAsync(x => x.UserId == userId && x.MovieId == movieId, ct);

        if (rating is null)
        {
            await tx.CommitAsync(ct);
            return await _db.MovieRatingAggregates
                .FirstOrDefaultAsync(x => x.MovieId == movieId, ct);
        }

        var oldScore = rating.Score;
        _db.Ratings.Remove(rating);

        var agg = await _db.MovieRatingAggregates
            .FirstOrDefaultAsync(x => x.MovieId == movieId, ct);

        if (agg is not null)
        {
            agg.RatingsCount -= 1;
            agg.RatingsSum -= oldScore;

            if (agg.RatingsCount <= 0)
            {
                _db.MovieRatingAggregates.Remove(agg);
                agg = null;
            }
            else
            {
                agg.AverageScore = (double)agg.RatingsSum / agg.RatingsCount;
                agg.UpdatedAt = DateTimeOffset.UtcNow;
            }
        }

        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return agg;
    }

    public async Task<(IReadOnlyList<MovieRatingAggregate> aggregates, IReadOnlyList<Domain.Entity.MovieRatings> userRatings)>
        GetSummariesAsync(Guid? userId, IReadOnlyList<Guid> movieIds, CancellationToken ct)
    {
        var aggs = await _db.MovieRatingAggregates
            .Where(x => movieIds.Contains(x.MovieId))
            .ToListAsync(ct);

        List<Domain.Entity.MovieRatings> userR = new();
        if (userId.HasValue)
        {
            userR = await _db.Ratings
                .Where(x => x.UserId == userId.Value && movieIds.Contains(x.MovieId))
                .ToListAsync(ct);
        }

        return (aggs, userR);
    }

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}