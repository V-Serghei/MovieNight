using Media.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Media.Infrastructure.Data;

public class MediaDbContext : DbContext
{
    public MediaDbContext(DbContextOptions<MediaDbContext> options) : base(options)
    {
    }

    public DbSet<MediaFile> MediaFiles => Set<MediaFile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MediaFile>(e =>
        {
            e.ToTable("MediaFiles");

            e.HasKey(x => x.Id);

            e.Property(x => x.FileName)
                .HasMaxLength(255)
                .IsRequired();

            e.Property(x => x.ContentType)
                .HasMaxLength(100)
                .IsRequired();

            e.Property(x => x.Length)
                .IsRequired();

            e.Property(x => x.Data)
                .IsRequired()
                .HasColumnType("varbinary(max)");

            e.Property(x => x.CreatedAt)
                .IsRequired();
        });
    }
}