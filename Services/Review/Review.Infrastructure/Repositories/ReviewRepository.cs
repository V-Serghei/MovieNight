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
    Task<Domain.Entities.Review?> IReviewRepository.FindByFilmIdAsync(string filmId, CancellationToken ct = default)
        => db.Reviews.SingleOrDefaultAsync(m => m.FilmId == filmId, ct);
}