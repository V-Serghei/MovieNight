using Access.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Access.Infrastructure.Configurations;

public class RolePolicyConfiguration : IEntityTypeConfiguration<RolePolicy>
{
    public void Configure(EntityTypeBuilder<RolePolicy> e)
    {
        e.HasKey(x => new { x.RoleId, x.PolicyId });
        e.HasIndex(x => new { x.RoleId, x.PolicyId }).IsUnique();
    }
}
