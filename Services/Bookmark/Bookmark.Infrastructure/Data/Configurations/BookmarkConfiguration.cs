using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookmark.Infrastructure.Data.Configurations;

public class BookmarkConfiguration : IEntityTypeConfiguration<Domain.Entity.Bookmark>
{
    public void Configure(EntityTypeBuilder<Domain.Entity.Bookmark> b)
    {
        b.ToTable("Bookmarks");

        b.HasKey(x => x.Id);

        b.Property(x => x.UserId).IsRequired();
        b.Property(x => x.MovieId).IsRequired();

        b.Property(x => x.Kind)
            .HasConversion<int>()
            .IsRequired();

        b.Property(x => x.MovieTitle).HasMaxLength(400).IsRequired();
        b.Property(x => x.MovieYear).IsRequired();

        b.Property(x => x.MovieDuration).HasMaxLength(50);
        b.Property(x => x.MoviePosterImage).HasMaxLength(512);

        b.Property(x => x.CreatedAt).IsRequired();
        b.Property(x => x.WatchedAt);
        b.Property(x => x.ExpiresAt);

        b.HasIndex(x => new { x.UserId, x.MovieId, x.Kind }).IsUnique();
        b.HasIndex(x => new { x.UserId, x.Kind, x.CreatedAt });
        b.HasIndex(x => new { x.Kind, x.ExpiresAt });
    }
}