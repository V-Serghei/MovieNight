using Microsoft.EntityFrameworkCore;

namespace Messages.Infrastructure.Data;

public class MessagesDbContext(DbContextOptions<MessagesDbContext> options) : DbContext(options)
{
    public DbSet<Domain.Entities.Messages> Messages => Set<Domain.Entities.Messages>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Domain.Entities.Messages>(e =>
        {
            e.HasKey(x => x.Id);
        });
    }
}