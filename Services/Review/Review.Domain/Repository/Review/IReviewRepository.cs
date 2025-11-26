namespace Review.Domain.Repository.Review;

public interface IReviewRepository
{
    Task<List<Entities.Review>> FindByFilmIdAsync(string filmId, CancellationToken ct = default);
    Task AddAsync(Entities.Review review, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
    
}