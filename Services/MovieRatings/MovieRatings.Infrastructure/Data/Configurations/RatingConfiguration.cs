using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MovieRatings.Infrastructure.Data.Configurations;

public class RatingConfiguration : IEntityTypeConfiguration<Domain.Entity.MovieRatings>
{
    public void Configure(EntityTypeBuilder<Domain.Entity.MovieRatings> b)
    {
        b.ToTable("Ratings");
        b.HasKey(x => x.Id);

        b.Property(x => x.UserId).IsRequired();
        b.Property(x => x.MovieId).IsRequired();

        b.Property(x => x.Score)
            .IsRequired();

        b.Property(x => x.CreatedAt).IsRequired();
        b.Property(x => x.UpdatedAt).IsRequired();

        b.HasIndex(x => new { x.UserId, x.MovieId }).IsUnique();
        b.HasIndex(x => x.MovieId);
    }
}