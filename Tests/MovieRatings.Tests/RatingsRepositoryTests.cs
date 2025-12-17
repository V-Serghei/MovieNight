using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MovieRatings.Domain.Entity;
using MovieRatings.Infrastructure.Data;
using MovieRatings.Infrastructure.Repositories;
using Xunit;

namespace MovieRatings.Tests;

public class RatingsRepositoryTests
{
    private static RatingsDbContext CreateContext()
        => RatingsDbContextFactory.CreateContext(Guid.NewGuid().ToString());

    [Fact]
    public async Task GetUserRatingAsync_Should_ReturnRating_WhenExists()
    {
        await using var db = CreateContext();
        var repo = new RatingsRepository(db);

        var userId = Guid.NewGuid();
        var movieId = Guid.NewGuid();

        var rating = new Domain.Entity.MovieRatings
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            MovieId = movieId,
            Score = 7,
            CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-10),
            UpdatedAt = DateTimeOffset.UtcNow.AddMinutes(-5)
        };

        db.Ratings.Add(rating);
        await db.SaveChangesAsync();

        var result = await repo.GetUserRatingAsync(userId, movieId, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(rating.Id);
        result.Score.Should().Be(7);
    }

    [Fact]
    public async Task GetUserRatingAsync_Should_ReturnNull_WhenNotExists()
    {
        await using var db = CreateContext();
        var repo = new RatingsRepository(db);

        var result = await repo.GetUserRatingAsync(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAggregateAsync_Should_ReturnAggregate_WhenExists()
    {
        await using var db = CreateContext();
        var repo = new RatingsRepository(db);

        var movieId = Guid.NewGuid();

        var agg = new MovieRatingAggregate
        {
            MovieId = movieId,
            RatingsCount = 3,
            RatingsSum = 21,
            AverageScore = 7.0,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        db.MovieRatingAggregates.Add(agg);
        await db.SaveChangesAsync();

        var result = await repo.GetAggregateAsync(movieId, CancellationToken.None);

        result.Should().NotBeNull();
        result!.MovieId.Should().Be(movieId);
        result.RatingsCount.Should().Be(3);
        result.AverageScore.Should().Be(7.0);
    }

    [Fact]
    public async Task GetAggregateAsync_Should_ReturnNull_WhenNotExists()
    {
        await using var db = CreateContext();
        var repo = new RatingsRepository(db);

        var result = await repo.GetAggregateAsync(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeNull();
    }

    // ---------- SetRatingAsync ----------

    [Fact]
    public async Task SetRatingAsync_Should_CreateRating_AndAggregate_WhenNoneExist()
    {
        await using var db = CreateContext();
        var repo = new RatingsRepository(db);

        var userId = Guid.NewGuid();
        var movieId = Guid.NewGuid();

        var (rating, aggregate) = await repo.SetRatingAsync(userId, movieId, 8, CancellationToken.None);

        rating.Should().NotBeNull();
        aggregate.Should().NotBeNull();

        rating.UserId.Should().Be(userId);
        rating.MovieId.Should().Be(movieId);
        rating.Score.Should().Be(8);
        rating.CreatedAt.Should().NotBe(default);
        rating.UpdatedAt.Should().NotBe(default);

        aggregate.MovieId.Should().Be(movieId);
        aggregate.RatingsCount.Should().Be(1);
        aggregate.RatingsSum.Should().Be(8);
        aggregate.AverageScore.Should().Be(8.0);

        var storedRating = await db.Ratings.SingleOrDefaultAsync(r => r.UserId == userId && r.MovieId == movieId);
        storedRating.Should().NotBeNull();

        var storedAgg = await db.MovieRatingAggregates.SingleOrDefaultAsync(a => a.MovieId == movieId);
        storedAgg.Should().NotBeNull();
        storedAgg!.RatingsCount.Should().Be(1);
    }

    [Fact]
    public async Task SetRatingAsync_Should_AddNewRating_AndUpdateExistingAggregate()
    {
        await using var db = CreateContext();
        var repo = new RatingsRepository(db);

        var movieId = Guid.NewGuid();
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();

        // существующий рейтинг + агрегат
        var existingRating = new Domain.Entity.MovieRatings
        {
            Id = Guid.NewGuid(),
            UserId = user1,
            MovieId = movieId,
            Score = 6,
            CreatedAt = DateTimeOffset.UtcNow.AddHours(-2),
            UpdatedAt = DateTimeOffset.UtcNow.AddHours(-2)
        };

        var agg = new MovieRatingAggregate
        {
            MovieId = movieId,
            RatingsCount = 1,
            RatingsSum = 6,
            AverageScore = 6.0,
            UpdatedAt = DateTimeOffset.UtcNow.AddHours(-2)
        };

        db.Ratings.Add(existingRating);
        db.MovieRatingAggregates.Add(agg);
        await db.SaveChangesAsync();

        // новый пользователь ставит оценку
        var (rating2, aggregate2) = await repo.SetRatingAsync(user2, movieId, 8, CancellationToken.None);

        rating2.UserId.Should().Be(user2);
        rating2.Score.Should().Be(8);

        aggregate2.MovieId.Should().Be(movieId);
        aggregate2.RatingsCount.Should().Be(2);
        aggregate2.RatingsSum.Should().Be(14);
        aggregate2.AverageScore.Should().Be(7.0);

        var storedAgg = await db.MovieRatingAggregates.SingleAsync(a => a.MovieId == movieId);
        storedAgg.RatingsCount.Should().Be(2);
        storedAgg.RatingsSum.Should().Be(14);
        storedAgg.AverageScore.Should().Be(7.0);
    }

    [Fact]
    public async Task SetRatingAsync_Should_UpdateExistingRating_AndRecalculateAggregate()
    {
        await using var db = CreateContext();
        var repo = new RatingsRepository(db);

        var movieId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var rating = new Domain.Entity.MovieRatings
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            MovieId = movieId,
            Score = 5,
            CreatedAt = DateTimeOffset.UtcNow.AddHours(-3),
            UpdatedAt = DateTimeOffset.UtcNow.AddHours(-3)
        };

        var agg = new MovieRatingAggregate
        {
            MovieId = movieId,
            RatingsCount = 1,
            RatingsSum = 5,
            AverageScore = 5.0,
            UpdatedAt = DateTimeOffset.UtcNow.AddHours(-3)
        };

        db.Ratings.Add(rating);
        db.MovieRatingAggregates.Add(agg);
        await db.SaveChangesAsync();

        var (updatedRating, updatedAgg) = await repo.SetRatingAsync(userId, movieId, 9, CancellationToken.None);

        updatedRating.Score.Should().Be(9);
        updatedAgg.RatingsCount.Should().Be(1);
        updatedAgg.RatingsSum.Should().Be(9);
        updatedAgg.AverageScore.Should().Be(9.0);

        var storedRating = await db.Ratings.SingleAsync(r => r.UserId == userId && r.MovieId == movieId);
        storedRating.Score.Should().Be(9);

        var storedAgg = await db.MovieRatingAggregates.SingleAsync(a => a.MovieId == movieId);
        storedAgg.RatingsSum.Should().Be(9);
        storedAgg.AverageScore.Should().Be(9.0);
    }

    [Fact]
    public async Task SetRatingAsync_Should_Throw_WhenScoreOutOfRange()
    {
        await using var db = CreateContext();
        var repo = new RatingsRepository(db);

        var userId = Guid.NewGuid();
        var movieId = Guid.NewGuid();

        Func<Task> actLow = async () =>
            await repo.SetRatingAsync(userId, movieId, 0, CancellationToken.None);

        Func<Task> actHigh = async () =>
            await repo.SetRatingAsync(userId, movieId, 11, CancellationToken.None);

        await actLow.Should().ThrowAsync<ArgumentOutOfRangeException>();
        await actHigh.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    // ---------- DeleteRatingAsync ----------

    [Fact]
    public async Task DeleteRatingAsync_Should_RemoveRating_AndUpdateAggregate_WhenMoreThanOneRating()
    {
        await using var db = CreateContext();
        var repo = new RatingsRepository(db);

        var movieId = Guid.NewGuid();
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();

        var r1 = new Domain.Entity.MovieRatings
        {
            Id = Guid.NewGuid(),
            UserId = user1,
            MovieId = movieId,
            Score = 6,
            CreatedAt = DateTimeOffset.UtcNow.AddHours(-2),
            UpdatedAt = DateTimeOffset.UtcNow.AddHours(-2)
        };

        var r2 = new Domain.Entity.MovieRatings
        {
            Id = Guid.NewGuid(),
            UserId = user2,
            MovieId = movieId,
            Score = 8,
            CreatedAt = DateTimeOffset.UtcNow.AddHours(-1),
            UpdatedAt = DateTimeOffset.UtcNow.AddHours(-1)
        };

        var agg = new MovieRatingAggregate
        {
            MovieId = movieId,
            RatingsCount = 2,
            RatingsSum = 14,
            AverageScore = 7.0,
            UpdatedAt = DateTimeOffset.UtcNow.AddHours(-1)
        };

        db.Ratings.AddRange(r1, r2);
        db.MovieRatingAggregates.Add(agg);
        await db.SaveChangesAsync();

        var resultAgg = await repo.DeleteRatingAsync(user1, movieId, CancellationToken.None);

        resultAgg.Should().NotBeNull();
        resultAgg!.RatingsCount.Should().Be(1);
        resultAgg.RatingsSum.Should().Be(8);
        resultAgg.AverageScore.Should().Be(8.0);

        var remainingRatings = await db.Ratings.Where(r => r.MovieId == movieId).ToListAsync();
        remainingRatings.Should().HaveCount(1);
        remainingRatings[0].UserId.Should().Be(user2);

        var storedAgg = await db.MovieRatingAggregates.SingleAsync(a => a.MovieId == movieId);
        storedAgg.RatingsCount.Should().Be(1);
        storedAgg.RatingsSum.Should().Be(8);
    }

    [Fact]
    public async Task DeleteRatingAsync_Should_RemoveAggregate_WhenLastRatingDeleted()
    {
        await using var db = CreateContext();
        var repo = new RatingsRepository(db);

        var movieId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var rating = new Domain.Entity.MovieRatings
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            MovieId = movieId,
            Score = 9,
            CreatedAt = DateTimeOffset.UtcNow.AddHours(-2),
            UpdatedAt = DateTimeOffset.UtcNow.AddHours(-2)
        };

        var agg = new MovieRatingAggregate
        {
            MovieId = movieId,
            RatingsCount = 1,
            RatingsSum = 9,
            AverageScore = 9.0,
            UpdatedAt = DateTimeOffset.UtcNow.AddHours(-2)
        };

        db.Ratings.Add(rating);
        db.MovieRatingAggregates.Add(agg);
        await db.SaveChangesAsync();

        var resultAgg = await repo.DeleteRatingAsync(userId, movieId, CancellationToken.None);

        resultAgg.Should().BeNull();

        var remainingRatings = await db.Ratings.Where(r => r.MovieId == movieId).ToListAsync();
        remainingRatings.Should().BeEmpty();

        var aggExists = await db.MovieRatingAggregates.AnyAsync(a => a.MovieId == movieId);
        aggExists.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteRatingAsync_Should_ReturnExistingAggregate_WhenRatingNotFound()
    {
        await using var db = CreateContext();
        var repo = new RatingsRepository(db);

        var movieId = Guid.NewGuid();

        var agg = new MovieRatingAggregate
        {
            MovieId = movieId,
            RatingsCount = 3,
            RatingsSum = 21,
            AverageScore = 7.0,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        db.MovieRatingAggregates.Add(agg);
        await db.SaveChangesAsync();

        var resultAgg = await repo.DeleteRatingAsync(Guid.NewGuid(), movieId, CancellationToken.None);

        resultAgg.Should().NotBeNull();
        resultAgg!.RatingsCount.Should().Be(3);
        resultAgg.RatingsSum.Should().Be(21);

        var storedAgg = await db.MovieRatingAggregates.SingleAsync(a => a.MovieId == movieId);
        storedAgg.RatingsCount.Should().Be(3);
    }

    [Fact]
    public async Task DeleteRatingAsync_Should_ReturnNull_WhenRatingAndAggregateNotFound()
    {
        await using var db = CreateContext();
        var repo = new RatingsRepository(db);

        var resultAgg = await repo.DeleteRatingAsync(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        resultAgg.Should().BeNull();
    }

    // ---------- GetSummariesAsync ----------

    [Fact]
    public async Task GetSummariesAsync_Should_ReturnAggregates_AndUserRatings_WhenUserProvided()
    {
        await using var db = CreateContext();
        var repo = new RatingsRepository(db);

        var userId = Guid.NewGuid();
        var otherUser = Guid.NewGuid();

        var movie1 = Guid.NewGuid();
        var movie2 = Guid.NewGuid();
        var movieOther = Guid.NewGuid();

        db.MovieRatingAggregates.AddRange(
            new MovieRatingAggregate
            {
                MovieId = movie1,
                RatingsCount = 2,
                RatingsSum = 16,
                AverageScore = 8.0,
                UpdatedAt = DateTimeOffset.UtcNow
            },
            new MovieRatingAggregate
            {
                MovieId = movie2,
                RatingsCount = 1,
                RatingsSum = 6,
                AverageScore = 6.0,
                UpdatedAt = DateTimeOffset.UtcNow
            },
            new MovieRatingAggregate
            {
                MovieId = movieOther,
                RatingsCount = 5,
                RatingsSum = 40,
                AverageScore = 8.0,
                UpdatedAt = DateTimeOffset.UtcNow
            }
        );

        db.Ratings.AddRange(
            new Domain.Entity.MovieRatings
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                MovieId = movie1,
                Score = 9,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            },
            new Domain.Entity.MovieRatings
            {
                Id = Guid.NewGuid(),
                UserId = otherUser,
                MovieId = movie1,
                Score = 7,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            },
            new Domain.Entity.MovieRatings
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                MovieId = movie2,
                Score = 6,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            }
        );

        await db.SaveChangesAsync();

        var movieIds = new[] { movie1, movie2 };

        var (aggregates, userRatings) = await repo.GetSummariesAsync(userId, movieIds, CancellationToken.None);

        aggregates.Should().HaveCount(2);
        aggregates.Select(a => a.MovieId).Should().BeEquivalentTo(movieIds);

        userRatings.Should().HaveCount(2);
        userRatings.All(r => r.UserId == userId).Should().BeTrue();
        userRatings.Select(r => r.MovieId).Should().BeEquivalentTo(movieIds);
    }

    [Fact]
    public async Task GetSummariesAsync_Should_ReturnAggregates_AndEmptyUserRatings_WhenUserIsNull()
    {
        await using var db = CreateContext();
        var repo = new RatingsRepository(db);

        var movie1 = Guid.NewGuid();
        var movie2 = Guid.NewGuid();

        db.MovieRatingAggregates.AddRange(
            new MovieRatingAggregate
            {
                MovieId = movie1,
                RatingsCount = 1,
                RatingsSum = 8,
                AverageScore = 8.0,
                UpdatedAt = DateTimeOffset.UtcNow
            },
            new MovieRatingAggregate
            {
                MovieId = movie2,
                RatingsCount = 2,
                RatingsSum = 14,
                AverageScore = 7.0,
                UpdatedAt = DateTimeOffset.UtcNow
            }
        );

        await db.SaveChangesAsync();

        var (aggregates, userRatings) = await repo.GetSummariesAsync(
            userId: null,
            movieIds: new[] { movie1, movie2 },
            CancellationToken.None);

        aggregates.Should().HaveCount(2);
        userRatings.Should().BeEmpty();
    }

    [Fact]
    public async Task SaveChangesAsync_Should_PersistChanges()
    {
        await using var db = CreateContext();
        var repo = new RatingsRepository(db);

        var rating = new Domain.Entity.MovieRatings
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            MovieId = Guid.NewGuid(),
            Score = 10,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        db.Ratings.Add(rating);

        await repo.SaveChangesAsync(CancellationToken.None);

        var stored = await db.Ratings.SingleOrDefaultAsync(r => r.Id == rating.Id);
        stored.Should().NotBeNull();
        stored!.Score.Should().Be(10);
    }
}
