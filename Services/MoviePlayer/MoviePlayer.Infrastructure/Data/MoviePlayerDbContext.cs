using MoviePlayer.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MoviePlayer.Infrastructure.Data.Migrations;

public class MoviePlayerDbContext(DbContextOptions<MoviePlayerDbContext> options) : DbContext(options)
{
    public DbSet<Movie> Movies => Set<Movie>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Movie>(e =>
        {
            e.HasKey(x => x.Id);
        });
    }
}