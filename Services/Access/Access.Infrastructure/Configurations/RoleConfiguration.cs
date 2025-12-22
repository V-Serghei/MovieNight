using Access.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Access.Infrastructure.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> e)
    {
        e.HasKey(x => x.Id);
        e.Property(x => x.Name).HasMaxLength(100).IsRequired();
        e.HasIndex(x => x.Name).IsUnique();
    }
}
