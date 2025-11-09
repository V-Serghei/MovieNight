using Auth.Domain.Entities;

namespace Auth.Infrastructure.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> e)
    {
        e.HasKey(x => x.Id);
        e.HasIndex(x => new { x.UserId, x.Token }).IsUnique();
        e.Property(x => x.Token).HasMaxLength(200).IsRequired();
    }
}
