using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Review.Domain.Entities;
using Review.Domain.Repository.Review;
using Review.Infrastructure.Data;
using Review.Infrastructure.Repositories;
using Xunit;

namespace Review.Tests;

public class ReviewRepositoryTests
{
    private static ReviewDbContext CreateContext()
        => ReviewDbContextFactory.CreateContext(Guid.NewGuid().ToString());

    [Fact]
    public async Task AddAsync_And_SaveChangesAsync_Should_PersistReview()
    {
        // Arrange
        await using var db = CreateContext();

        IReviewRepository repo = new ReviewRepository(db);

        var review = new Domain.Entities.Review
        {
            FilmId = "tt1375666",
            Film = "Inception",
            User = "Serghei",
            UserId = "user-1",
            Text = "Great movie!",
            Date = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc)
        };

        // Act
        await repo.AddAsync(review);
        await repo.SaveChangesAsync(); // ← работает, потому что repo = интерфейс

        // Assert
        var stored = await db.Reviews.SingleOrDefaultAsync(r => r.Id == review.Id);
        stored.Should().NotBeNull();
        stored!.FilmId.Should().Be("tt1375666");
        stored.Film.Should().Be("Inception");
        stored.User.Should().Be("Serghei");
        stored.Text.Should().Be("Great movie!");
        stored.Date.Should().Be(review.Date);
    }

    [Fact]
    public async Task FindByFilmIdAsync_Should_ReturnOnlyReviewsForGivenFilm_OrderedByDateDesc()
    {
        // Arrange
        await using var db = CreateContext();
        var repo = new ReviewRepository(db);

        var filmId = "tt1375666";

        var older = new Domain.Entities.Review
        {
            FilmId = filmId,
            Film = "Inception",
            User = "User1",
            Text = "Old review",
            Date = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc)
        };

        var newer = new Domain.Entities.Review
        {
            FilmId = filmId,
            Film = "Inception",
            User = "User2",
            Text = "New review",
            Date = new DateTime(2024, 1, 2, 12, 0, 0, DateTimeKind.Utc)
        };

        var otherFilm = new Domain.Entities.Review
        {
            FilmId = "tt0133093",
            Film = "The Matrix",
            User = "User3",
            Text = "Matrix review",
            Date = new DateTime(2024, 1, 3, 12, 0, 0, DateTimeKind.Utc)
        };

        db.Reviews.AddRange(older, newer, otherFilm);
        await db.SaveChangesAsync();

        // Act
        var list = await repo.FindByFilmIdAsync(filmId);

        // Assert
        list.Should().HaveCount(2);
        list.Select(r => r.FilmId).Distinct().Should().ContainSingle().Which.Should().Be(filmId);

        // порядок по дате: сначала самый новый
        list[0].Text.Should().Be("New review");
        list[1].Text.Should().Be("Old review");
    }

    [Fact]
    public async Task FindByFilmIdAsync_Should_ReturnEmptyList_WhenNoReviewsForFilm()
    {
        // Arrange
        await using var db = CreateContext();
        var repo = new ReviewRepository(db);

        // немного мусора для других фильмов
        db.Reviews.AddRange(
            new Domain.Entities.Review
            {
                FilmId = "tt0133093",
                Film = "The Matrix",
                User = "U1",
                Text = "Matrix review",
                Date = DateTime.UtcNow.AddDays(-1)
            },
            new Domain.Entities.Review
            {
                FilmId = "tt0137523",
                Film = "Fight Club",
                User = "U2",
                Text = "Fight Club review",
                Date = DateTime.UtcNow
            }
        );
        await db.SaveChangesAsync();

        // Act
        var list = await repo.FindByFilmIdAsync("tt1375666"); // Inception, не добавлен

        // Assert
        list.Should().NotBeNull();
        list.Should().BeEmpty();
    }
}
