using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MoviePlayer.Domain.Entities;
using MoviePlayer.Domain.Enums;
using MoviePlayer.Infrastructure.Data;
using MoviePlayer.Infrastructure.Repositories;
using Xunit;

namespace MoviePlayer.Tests;

public class MovieRepositoryTests
{
    private static MoviePlayerDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<MoviePlayerDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new MoviePlayerDbContext(options);
    }

    [Fact]
    public async Task CreateAsync_Should_SetTimestamps_And_AttachGenres_And_SaveMovie()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = CreateContext(dbName);

        // существующий жанр в базе
        var existingGenre = new Genre { Name = "Action" };
        db.Genres.Add(existingGenre);
        await db.SaveChangesAsync();

        var movie = new Movie
        {
            Id = Guid.NewGuid(),
            Title = "Inception",
            ProductionYear = 2010,
            Director = "Christopher Nolan",
            Category = MovieCategory.Anime,
            Genres = new List<Genre>(),
            MovieCards = new List<MovieCard>(),
            InterestingFacts = new List<MovieFact>()
        };

        var genreNames = new[] { "Action", "Drama", " ", null };

        var repo = new MovieRepository(db);

        // Act
        var result = await repo.CreateAsync(movie, genreNames);

        // Assert
        result.CreatedAt.Should().NotBe(default);
        result.UpdatedAt.Should().Be(result.CreatedAt);

        result.Genres.Should().HaveCount(2);
        result.Genres.Should().ContainSingle(g => g.Name == "Action" && g.Id == existingGenre.Id);
        result.Genres.Should().ContainSingle(g => g.Name == "Drama");

        db.Movies.Count().Should().Be(1);
        db.Genres.Count().Should().Be(2); // Action + Drama
    }

    [Fact]
    public async Task FindByIdAsync_Should_ReturnMovieWithNavigationProperties()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = CreateContext(dbName);

        var movieId = Guid.NewGuid();

        var genre = new Genre { Name = "Sci-Fi" };
        var card = new MovieCard
        {
            Id = Guid.NewGuid(),
            Title = "Main card",
            MovieId = movieId,
            ImageUrl = "https://example.com/image.jpg"
        };
        var fact = new MovieFact
        {
            Id = Guid.NewGuid(),
            MovieId = movieId,
            FactName = "Shot in IMAX",
            Text = "Shot in IMAX"
        };

        var movie = new Movie
        {
            Id = movieId,
            Title = "Interstellar",
            ProductionYear = 2014,
            Director = "Christopher Nolan",
            Category = MovieCategory.Cartoon,
            Genres = new List<Genre> { genre },
            MovieCards = new List<MovieCard> { card },
            InterestingFacts = new List<MovieFact> { fact }
        };

        db.Movies.Add(movie);
        await db.SaveChangesAsync();

        var repo = new MovieRepository(db);

        // Act
        var result = await repo.FindByIdAsync(movieId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(movieId);

        result.Genres.Should().HaveCount(1);
        result.Genres.First().Name.Should().Be("Sci-Fi");

        result.MovieCards.Should().HaveCount(1);
        result.InterestingFacts.Should().HaveCount(1);
        result.InterestingFacts.First().FactName.Should().Be("Shot in IMAX");
    }

    [Fact]
    public async Task GetByCategoryAsync_Should_ReturnOnlyMoviesWithGivenCategory()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = CreateContext(dbName);

        db.Movies.AddRange(
            new Movie
            {
                Id = Guid.NewGuid(),
                Title = "Movie 1",
                ProductionYear = 2020,
                Director = "Dir 1",
                Category = MovieCategory.Anime,
                Genres = new List<Genre>(),
                MovieCards = new List<MovieCard>(),
                InterestingFacts = new List<MovieFact>()
            },
            new Movie
            {
                Id = Guid.NewGuid(),
                Title = "Movie 2",
                ProductionYear = 2021,
                Director = "Dir 2",
                Category = MovieCategory.Serial,
                Genres = new List<Genre>(),
                MovieCards = new List<MovieCard>(),
                InterestingFacts = new List<MovieFact>()
            }
        );

        await db.SaveChangesAsync();

        var repo = new MovieRepository(db);

        // Act
        var result = await repo.GetByCategoryAsync(MovieCategory.Anime);

        // Assert
        result.Should().HaveCount(1);
        result[0].Title.Should().Be("Movie 1");
        result[0].Category.Should().Be(MovieCategory.Anime);
    }

    [Fact]
    public async Task FindByInfoAsync_Should_ReturnMovie_WhenAllFieldsMatch()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = CreateContext(dbName);

        var movie = new Movie
        {
            Id = Guid.NewGuid(),
            Title = "The Matrix",
            ProductionYear = 1999,
            Director = "Wachowski",
            Category = MovieCategory.Cartoon,
            Genres = new List<Genre>(),
            MovieCards = new List<MovieCard>(),
            InterestingFacts = new List<MovieFact>()
        };

        db.Movies.Add(movie);
        await db.SaveChangesAsync();

        var repo = new MovieRepository(db);

        // Act
        var result = await repo.FindByInfoAsync("The Matrix", 1999, "Wachowski");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(movie.Id);
    }

    [Fact]
    public async Task AddAsync_And_SaveChangesAsync_Should_PersistMovie()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = CreateContext(dbName);

        var repo = new MovieRepository(db);

        var movie = new Movie
        {
            Id = Guid.NewGuid(),
            Title = "Test Movie",
            ProductionYear = 2024,
            Director = "Me",
            Category = MovieCategory.Film,
            Genres = new List<Genre>(),
            MovieCards = new List<MovieCard>(),
            InterestingFacts = new List<MovieFact>()
        };

        // Act
        await repo.AddAsync(movie);
        await repo.SaveChangesAsync();

        // Assert
        var stored = await db.Movies.SingleOrDefaultAsync(m => m.Id == movie.Id);
        stored.Should().NotBeNull();
        stored!.Title.Should().Be("Test Movie");
    }
}
