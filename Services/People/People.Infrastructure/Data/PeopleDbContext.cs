using Microsoft.EntityFrameworkCore;
using People.Domain.Entity;

namespace People.Infrastructure.Data;

public class PeopleDbContext : DbContext
{
    public PeopleDbContext(DbContextOptions<PeopleDbContext> options) : base(options) { }

    public DbSet<Person> People => Set<Person>();
    public DbSet<MovieCredit> MovieCredits => Set<MovieCredit>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Person>(b =>
        {
            b.HasKey(p => p.Id);
            b.Property(p => p.FullName).IsRequired().HasMaxLength(200);
            b.Property(p => p.KnownForDepartment).HasMaxLength(100);
            b.Property(p => p.Country).HasMaxLength(100);
        });

        modelBuilder.Entity<MovieCredit>(b =>
        {
            b.HasKey(mc => mc.Id);
            b.Property(mc => mc.CharacterName).HasMaxLength(200);

            b.HasIndex(mc => mc.MovieId);
            b.HasIndex(mc => mc.PersonId);

            b.HasOne<Person>()
                .WithMany()
                .HasForeignKey(mc => mc.PersonId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}