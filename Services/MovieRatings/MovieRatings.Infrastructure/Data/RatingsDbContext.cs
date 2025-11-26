using Microsoft.EntityFrameworkCore;
using MovieRatings.Domain.Entity;

namespace MovieRatings.Infrastructure.Data;

public class RatingsDbContext : DbContext
{
    public RatingsDbContext(DbContextOptions<RatingsDbContext> options) : base(options)
    {
    }

    public DbSet<Domain.Entity.MovieRatings> Ratings => Set<Domain.Entity.MovieRatings>();
    public DbSet<MovieRatingAggregate> MovieRatingAggregates => Set<MovieRatingAggregate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new Configurations.RatingConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.MovieRatingAggregateConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}