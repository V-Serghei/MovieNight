using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MovieRatings.Domain.Entity;

namespace MovieRatings.Infrastructure.Data.Configurations;

public class MovieRatingAggregateConfiguration : IEntityTypeConfiguration<MovieRatingAggregate>
{
    public void Configure(EntityTypeBuilder<MovieRatingAggregate> b)
    {
        b.ToTable("MovieRatingAggregates");
        b.HasKey(x => x.MovieId);

        b.Property(x => x.RatingsCount).IsRequired();
        b.Property(x => x.RatingsSum).IsRequired();
        b.Property(x => x.AverageScore)
            .HasColumnType("float")
            .IsRequired();

        b.Property(x => x.UpdatedAt).IsRequired();
    }
}