using MoviePlayer.Domain.Entities;
using MoviePlayer.Domain.Repository.Movie;
using Microsoft.EntityFrameworkCore;
using MoviePlayer.Domain.Enums;

namespace MoviePlayer.Domain.Repository.Movie;

public interface IMovieRepository
{
    Task<Entities.Movie?> FindByIdAsync(Guid id, CancellationToken ct = default);
    Task<Entities.Movie?> FindByInfoAsync(string title, int year, string director, CancellationToken ct = default);
    Task<List<Entities.Movie>> GetAllAsync(CancellationToken ct = default);
    Task<List<Entities.Movie>> GetByCategoryAsync(MovieCategory category, CancellationToken ct = default);

    Task AddAsync(Entities.Movie movie, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);

    Task<Entities.Movie> CreateAsync(Entities.Movie movie, IEnumerable<string> genreNames, CancellationToken ct = default);
}