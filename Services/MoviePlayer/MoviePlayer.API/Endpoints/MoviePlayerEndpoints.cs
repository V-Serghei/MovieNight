using System.Text.Json;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using MoviePlayer.Domain.Entities;
using MoviePlayer.Domain.Repository.Movie;
using MoviePlayer.API.DTO;
using MoviePlayer.Domain.Enums;
using MoviePlayer.Infrastructure.Data;

namespace MoviePlayer.API.Endpoints;

public static class MoviePlayerEndpoints
{
    public static IEndpointRouteBuilder MapMoviesEndpoints(this IEndpointRouteBuilder routes)
    {
        var g = routes.MapGroup("/movies").WithTags("Movies");

        g.MapGet("/", async (IMovieRepository repo, CancellationToken ct) =>
        {
            var all = await repo.GetAllAsync(ct);
            return Results.Ok(all);
        });

        g.MapGet("/by-category/{category}", async (string category, IMovieRepository repo, CancellationToken ct) =>
        {
            if (!Enum.TryParse<MovieCategory>(category, ignoreCase: true, out var cat))
                return Results.BadRequest($"Unknown category: {category}");

            var list = await repo.GetByCategoryAsync(cat, ct);
            return Results.Ok(list);
        });

        g.MapGet("/{id:guid}", async (Guid id, IMovieRepository repo, CancellationToken ct) =>
        {
            var movie = await repo.FindByIdAsync(id, ct);
            return movie is not null ? Results.Ok(movie) : Results.NotFound();
        });

        g.MapGet("/search", async (string title, int year, string director, IMovieRepository repo, CancellationToken ct) =>
        {
            var movie = await repo.FindByInfoAsync(title, year, director, ct);
            return movie is not null ? Results.Ok(movie) : Results.NotFound();
        });

        g.MapPost("/", async (MovieDTO movieDto, IMovieRepository repo, CancellationToken ct) =>
        {
            var movie = new Movie
            {
                Id = Guid.NewGuid(),
                Title = movieDto.Title,
                Category = movieDto.Category,
                PosterImage = movieDto.PosterImage,
                Quote = movieDto.Quote,
                Description = movieDto.Description,
                ProductionYear = movieDto.ProductionYear,
                Country = movieDto.Country,
                Director = movieDto.Director,
                Duration = movieDto.Duration,
                Certificate = movieDto.Certificate,
                ProductionCompany = movieDto.ProductionCompany,
                Budget = movieDto.Budget,
                GrossWorldwide = movieDto.GrossWorldwide,
                Language = movieDto.Language
            };

            if (movieDto.Cards is { Count: > 0 })
            {
                foreach (var c in movieDto.Cards)
                {
                    movie.MovieCards.Add(new MovieCard
                    {
                        Id = Guid.NewGuid(),
                        ImageUrl = c.ImageUrl,
                        Title = c.Title,
                        Description = c.Description
                    });
                }
            }

            if (movieDto.Facts is { Count: > 0 })
            {
                foreach (var f in movieDto.Facts)
                {
                    movie.InterestingFacts.Add(new MovieFact
                    {
                        Id = Guid.NewGuid(),
                        FactName = f.FactName,
                        Text = f.Text
                    });
                }
            }

            var created = await repo.CreateAsync(movie, movieDto.Genre, ct);
            return Results.Created($"/movies/{created.Id}", created);
        });

        // SEED из JSON
        g.MapPost("/seed", async (SeedMoviesDto dto, IMovieRepository repo, CancellationToken ct) =>
        {
            if (!File.Exists(dto.JsonPath))
                return Results.BadRequest($"JSON file not found at {dto.JsonPath}");

            var json = await File.ReadAllTextAsync(dto.JsonPath, ct);
            var movies = JsonSerializer.Deserialize<List<SeedMovieJsonModel>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (movies is null || movies.Count == 0)
                return Results.BadRequest("No movies found in JSON.");

            int added = 0;

            foreach (var m in movies)
            {
                var director = m.Director ?? string.Empty;
                var exists = await repo.FindByInfoAsync(m.Title, m.ProductionYear, director, ct);
                if (exists is not null)
                    continue;

                var movie = new Movie
                {
                    Id = Guid.NewGuid(),
                    Title = m.Title,
                    Category = m.Category,
                    PosterImage = m.PosterImage,
                    Quote = m.Quote,
                    Description = m.Description,
                    ProductionYear = m.ProductionYear,
                    Country = m.Country,
                    Director = m.Director,
                    Duration = m.Duration,
                    Certificate = m.Certificate,
                    ProductionCompany = m.ProductionCompany,
                    Budget = m.Budget,
                    GrossWorldwide = m.GrossWorldwide,
                    Language = m.Language
                };

                if (m.Cards is { Count: > 0 })
                {
                    foreach (var c in m.Cards)
                    {
                        movie.MovieCards.Add(new MovieCard
                        {
                            Id = Guid.NewGuid(),
                            ImageUrl = c.ImageUrl,
                            Title = c.Title,
                            Description = c.Description
                        });
                    }
                }

                if (m.Facts is { Count: > 0 })
                {
                    foreach (var f in m.Facts)
                    {
                        movie.InterestingFacts.Add(new MovieFact
                        {
                            Id = Guid.NewGuid(),
                            FactName = f.FactName,
                            Text = f.Text
                        });
                    }
                }

                await repo.CreateAsync(movie, m.Genre, ct);
                added++;
            }

            return Results.Ok(new { added, total = movies.Count });
        });

        return routes;
    }

    private class SeedMovieJsonModel
    {
        public string Title { get; set; } = default!;
        public MovieCategory Category { get; set; }
        public string PosterImage { get; set; } = "";
        public string Quote { get; set; } = "";
        public string Description { get; set; } = "";
        public int ProductionYear { get; set; }
        public string Country { get; set; } = "";
        public string? Director { get; set; }
        public string Duration { get; set; } = "";
        public string Certificate { get; set; } = "";
        public string ProductionCompany { get; set; } = "";
        public string Budget { get; set; } = "";
        public string GrossWorldwide { get; set; } = "";
        public string Language { get; set; } = "";
        public List<string> Genre { get; set; } = new();
        public List<MovieCardDTO> Cards { get; set; } = new();
        public List<MovieFactDTO> Facts { get; set; } = new();
    }
}