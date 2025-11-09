using Access.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Access.Infrastructure.Configurations;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> e)
    {
        e.HasKey(x => new { x.UserId, x.RoleId });
        e.HasIndex(x => new { x.UserId, x.RoleId }).IsUnique();
    }
}
