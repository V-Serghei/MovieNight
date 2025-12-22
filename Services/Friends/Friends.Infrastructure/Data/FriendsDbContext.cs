using Microsoft.EntityFrameworkCore;

namespace Friends.Infrastructure;

public class FriendsDbContext(DbContextOptions<FriendsDbContext> options) : DbContext(options)
{
    public DbSet<Domain.Entities.Friends> Friends => Set<Domain.Entities.Friends>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Domain.Entities.Friends>(e =>
        {
            e.HasKey(x => new { x.IdUser, x.IdFriend });
        });
    }
}