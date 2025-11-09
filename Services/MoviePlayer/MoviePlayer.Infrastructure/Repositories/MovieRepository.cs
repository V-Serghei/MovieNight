using Microsoft.EntityFrameworkCore;
using MoviePlayer.Domain.Entities;
using MoviePlayer.Domain.Repository.Movie;
using MoviePlayer.Infrastructure.Data.Migrations;

namespace MoviePlayer.Infrastructure.Repositories;

public class MovieRepository(MoviePlayerDbContext db) : IMovieRepository
{
    public Task<Movie?> FindByIdAsync(Guid id, CancellationToken ct = default)
        => db.Movies.SingleOrDefaultAsync(m => m.Id == id, ct);

    public Task<Movie?> FindByInfoAsync(string title, int year, string director, CancellationToken ct = default)
        => db.Movies.SingleOrDefaultAsync(
            m => m.Title == title &&
                 m.ProductionYear.Year == year &&
                 m.Director == director, ct);

    public async Task<List<Movie>> GetAllAsync(CancellationToken ct = default)
        => await db.Movies.ToListAsync(ct);

    public Task AddAsync(Movie movie, CancellationToken ct = default)
        => db.Movies.AddAsync(movie, ct).AsTask();

    public Task SaveChangesAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);
}