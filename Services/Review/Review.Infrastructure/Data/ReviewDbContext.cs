using Microsoft.EntityFrameworkCore;

namespace Review.Infrastructure.Data;

public class ReviewDbContext(DbContextOptions<ReviewDbContext> options) : DbContext(options)
{
    public DbSet<Domain.Entities.Review> Reviews => Set<Domain.Entities.Review>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Domain.Entities.Review>(e =>
        {
            e.HasKey(x => x.Id);
        });
    }
}