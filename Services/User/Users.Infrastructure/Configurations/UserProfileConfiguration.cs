using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Users.Core.Entities;

namespace Users.Infrastructure.Configurations;

public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> b)
    {
        b.ToTable("UserProfiles");

        // PK = FK to Users.Id
        b.HasKey(p => p.UserId);

        b.HasOne(p => p.User)
            .WithOne(u => u.Profile)
            .HasForeignKey<UserProfile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Property(p => p.UserName).HasMaxLength(64);
        b.Property(p => p.FirstName).HasMaxLength(64);
        b.Property(p => p.LastName).HasMaxLength(64);
        b.Property(p => p.Gender).HasMaxLength(16);
        b.Property(p => p.PhoneNumber).HasMaxLength(32);
        b.Property(p => p.Country).HasMaxLength(64);

        b.Property(p => p.Facebook).HasMaxLength(256);
        b.Property(p => p.Twitter).HasMaxLength(256);
        b.Property(p => p.Instagram).HasMaxLength(256);
        b.Property(p => p.GitHub).HasMaxLength(256);
        b.Property(p => p.Quote).HasMaxLength(256);
        b.Property(p => p.AvatarMediaId).HasMaxLength(128);
    }
}