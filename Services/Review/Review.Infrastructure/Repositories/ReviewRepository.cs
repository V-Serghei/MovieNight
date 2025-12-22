using Microsoft.EntityFrameworkCore;
using Review.Domain.Repository.Review;
using Review.Infrastructure.Data;
using Review.Domain.Entities;
//using Review.Infrastructure.Data.Migrations;

namespace Review.Infrastructure.Repositories;

public class ReviewRepository(ReviewDbContext db) : IReviewRepository
{

    public Task AddAsync(Domain.Entities.Review review, CancellationToken ct = default)
        => db.Reviews.AddAsync(review, ct).AsTask();
    Task IReviewRepository.SaveChangesAsync(CancellationToken ct)
        => db.SaveChangesAsync(ct);
    public Task<List<Domain.Entities.Review>> FindByFilmIdAsync(string filmId, CancellationToken ct = default)
        => db.Reviews
            .Where(r => r.FilmId == filmId)
            .OrderByDescending(r => r.Date)
            .ToListAsync(ct);
}