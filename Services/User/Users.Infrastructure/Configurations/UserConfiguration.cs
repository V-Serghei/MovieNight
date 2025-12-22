using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Users.Core.Entities;

namespace Users.Infrastructure.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> e)
    {
        e.HasKey(x => x.Id);
        e.Property(x => x.Email).HasMaxLength(320).IsRequired();
        e.Property(x => x.EmailNormalized).HasMaxLength(320).IsRequired();
        e.HasIndex(x => x.EmailNormalized).IsUnique();
        e.Property(x => x.PasswordHash).IsRequired().HasMaxLength(200);
        e.Property(x => x.PasswordSalt).IsRequired().HasMaxLength(100);
        e.Property(x => x.DisplayName).HasMaxLength(200);
    }
}