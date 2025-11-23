using Microsoft.EntityFrameworkCore;
using MoviePlayer.Domain.Entities;
using MoviePlayer.Domain.Enums;
using MoviePlayer.Domain.Repository.Movie;
using MoviePlayer.Infrastructure.Data;
using MoviePlayer.Infrastructure.Data.Migrations;

namespace MoviePlayer.Infrastructure.Repositories;

public class MovieRepository(MoviePlayerDbContext db) : IMovieRepository
{
    public Task<Movie?> FindByIdAsync(Guid id, CancellationToken ct = default)
        => db.Movies
            .Include(m => m.Genres)
            .Include(m => m.MovieCards)
            .Include(m => m.InterestingFacts)
            .SingleOrDefaultAsync(m => m.Id == id, ct);

    public Task<Movie?> FindByInfoAsync(string title, int year, string director, CancellationToken ct = default)
        => db.Movies
            .Include(m => m.Genres)
            .Include(m => m.MovieCards)
            .Include(m => m.InterestingFacts)
            .SingleOrDefaultAsync(
                m => m.Title == title &&
                     m.ProductionYear == year &&
                     m.Director == director, ct);

    public async Task<List<Movie>> GetAllAsync(CancellationToken ct = default)
        => await db.Movies
            .Include(m => m.Genres)
            .Include(m => m.MovieCards)
            .Include(m => m.InterestingFacts)
            .ToListAsync(ct);

    public Task<List<Movie>> GetByCategoryAsync(MovieCategory category, CancellationToken ct = default)
        => db.Movies
            .Where(m => m.Category == category)
            .Include(m => m.Genres)
            .Include(m => m.MovieCards)
            .Include(m => m.InterestingFacts)
            .ToListAsync(ct);

    public Task AddAsync(Movie movie, CancellationToken ct = default)
        => db.Movies.AddAsync(movie, ct).AsTask();

    public Task SaveChangesAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);

    public async Task<Movie> CreateAsync(Movie movie, IEnumerable<string> genreNames, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        movie.CreatedAt = now;
        movie.UpdatedAt = now;

        // жанры
        if (genreNames is not null)
        {
            foreach (var raw in genreNames)
            {
                var name = raw?.Trim();
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                var genre = await db.Genres.FirstOrDefaultAsync(g => g.Name == name, ct);
                if (genre is null)
                {
                    genre = new Genre { Name = name };
                    await db.Genres.AddAsync(genre, ct);
                }

                movie.Genres.Add(genre);
            }
        }

        await db.Movies.AddAsync(movie, ct);
        await db.SaveChangesAsync(ct);

        return movie;
    }
}