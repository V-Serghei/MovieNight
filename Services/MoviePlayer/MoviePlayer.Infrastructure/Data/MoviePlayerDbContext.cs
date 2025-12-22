using Microsoft.EntityFrameworkCore;
using MoviePlayer.Domain.Entities;

namespace MoviePlayer.Infrastructure.Data;

public class MoviePlayerDbContext(DbContextOptions<MoviePlayerDbContext> options) : DbContext(options)
{
   public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<MovieCard> MovieCards => Set<MovieCard>();
    public DbSet<MovieFact> MovieFacts => Set<MovieFact>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<Movie>(e =>
        {
            e.HasKey(x => x.Id);

            e.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(256);

            e.Property(x => x.PosterImage).HasMaxLength(1024);
            e.Property(x => x.Country).HasMaxLength(128);
            e.Property(x => x.Language).HasMaxLength(64);
            e.Property(x => x.Director).HasMaxLength(256);
            e.Property(x => x.Certificate).HasMaxLength(64);
            e.Property(x => x.ProductionCompany).HasMaxLength(256);
            e.Property(x => x.Budget).HasMaxLength(128);
            e.Property(x => x.GrossWorldwide).HasMaxLength(128);
            e.Property(x => x.Location).HasMaxLength(256);

            e.HasMany(m => m.Genres)
                .WithMany(g => g.Movies)
                .UsingEntity(j =>
                {
                    j.ToTable("MovieGenres");
                });

            e.HasMany(m => m.MovieCards)
                .WithOne(c => c.Movie)
                .HasForeignKey(c => c.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(m => m.InterestingFacts)
                .WithOne(f => f.Movie)
                .HasForeignKey(f => f.MovieId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<Genre>(e =>
        {
            e.HasKey(g => g.Id);
            e.Property(g => g.Name)
                .IsRequired()
                .HasMaxLength(64);
            e.HasIndex(g => g.Name).IsUnique();
        });

        b.Entity<MovieCard>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.ImageUrl)
                .IsRequired()
                .HasMaxLength(1024);
            e.Property(c => c.Title)
                .HasMaxLength(256);
        });

        b.Entity<MovieFact>(e =>
        {
            e.HasKey(f => f.Id);
            e.Property(f => f.FactName)
                .IsRequired()
                .HasMaxLength(256);
        });
    }
}