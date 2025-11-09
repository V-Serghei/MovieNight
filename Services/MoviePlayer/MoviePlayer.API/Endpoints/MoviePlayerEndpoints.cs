using System.Text.Json;
using Auth.API.DTO;
using Microsoft.AspNetCore.Routing;
using MoviePlayer.Domain.Entities;
using MoviePlayer.Domain.Repository.Movie;
using MoviePlayer.API.DTO;

namespace MoviePlayer.API.Endpoints;

public static class MoviePlayerEndpoints
{
     public static IEndpointRouteBuilder MapMoviesEndpoints(this IEndpointRouteBuilder routes)
    {
        var g = routes.MapGroup("/movies").WithTags("Movies");
        //Получить все фильмы
        g.MapGet("/", async (IMovieRepository repo, CancellationToken ct) =>
        {
            var all = await repo.GetAllAsync(ct);
            return Results.Ok(all);
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
                Title = movieDto.Title,
                Category = movieDto.Category,
                PosterImage = movieDto.PosterImage,
                Quote = movieDto.Quote,
                Description = movieDto.Description,
                ProductionYear = movieDto.ProductionYear,
                ProductionYearS = movieDto.ProductionYearS,
                Country = movieDto.Country,
                Director = movieDto.Director,
                Duration = movieDto.Duration,
                Certificate = movieDto.Certificate,
                ProductionCompany = movieDto.ProductionCompany,
                Budget = movieDto.Budget,
                Genre = movieDto.Genre
            };
            await repo.AddAsync(movie, ct);
            await repo.SaveChangesAsync(ct);
            return Results.Created($"/movies/{movie.Id}", movie);
        });
        
        g.MapPost("/seed", async (SeedMoviesDto dto, IMovieRepository repo, CancellationToken ct) =>
        {
            if (!File.Exists(dto.JsonPath))
                return Results.BadRequest($"JSON file not found at {dto.JsonPath}");

            var json = await File.ReadAllTextAsync(dto.JsonPath, ct);
            var movies = JsonSerializer.Deserialize<List<Movie>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (movies is null || movies.Count == 0)
                return Results.BadRequest("No movies found in JSON.");

            int added = 0;
            foreach (var m in movies)
            {
                var exists = await repo.FindByInfoAsync(m.Title, m.ProductionYear.Year, m.Director, ct);
                if (exists is not null)
                    continue;

                await repo.AddAsync(m, ct);
                added++;
            }

            await repo.SaveChangesAsync(ct);
            return Results.Ok(new { added, total = movies.Count });
        });
        
        // //Получить только сериалы
        // g.MapGet("/cartoons", async (IMovieRepository movies, CancellationToken ct) =>
        // {
        //     var list = await movies.FindByCategotyAsync("cartoons", ct);
        //     return Results.Ok(list);
        // });
        // // 🔹 Получить только сериалы
        // g.MapGet("/series", async (IMovieRepository movies, CancellationToken ct) =>
        // {
        //     var list = await movies.GetByCategoryAsync("series", ct);
        //     return Results.Ok(list);
        // });
        //
        // // 🔹 Получить только аниме
        // g.MapGet("/anime", async (IMovieRepository movies, CancellationToken ct) =>
        // {
        //     var list = await movies.GetByCategoryAsync("anime", ct);
        //     return Results.Ok(list);
        // });
        //
        // // 🔹 Получить рандомный фильм
        // g.MapGet("/random", async (IMovieRepository movies, CancellationToken ct) =>
        // {
        //     var random = await movies.GetRandomAsync(ct);
        //     return random is not null ? Results.Ok(random) : Results.NotFound();
        // });
        return routes;
    }
}