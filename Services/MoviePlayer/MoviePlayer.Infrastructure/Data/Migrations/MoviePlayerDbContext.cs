using MoviePlayer.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MoviePlayer.Infrastructure.Data.Migrations;

public class MoviePlayerDbContext(DbContextOptions<MoviePlayerDbContext> options) : DbContext(options)
{
    public DbSet<Movie> Movies => Set<Movie>();
    //public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Movie>(e =>
        {
            e.HasKey(x => x.Id);
            //e.HasIndex(x => x.Email).IsUnique();
            //e.Property(x => x.Email).HasMaxLength(320).IsRequired();
        });
    }
}