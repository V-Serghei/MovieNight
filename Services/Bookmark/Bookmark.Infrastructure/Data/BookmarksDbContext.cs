using Microsoft.EntityFrameworkCore;

namespace Bookmark.Infrastructure.Data;

public class BookmarksDbContext : DbContext
{
    public BookmarksDbContext(DbContextOptions<BookmarksDbContext> options) : base(options)
    {
    }

    public DbSet<Domain.Entity.Bookmark> Bookmarks => Set<Domain.Entity.Bookmark>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new Configurations.BookmarkConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}