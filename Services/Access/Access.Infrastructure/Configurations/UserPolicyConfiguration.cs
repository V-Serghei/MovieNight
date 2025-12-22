using Access.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Access.Infrastructure.Configurations;

public class UserPolicyConfiguration : IEntityTypeConfiguration<UserPolicy>
{
    public void Configure(EntityTypeBuilder<UserPolicy> e)
    {
        e.HasKey(x => new { x.UserId, x.PolicyId });
        e.HasIndex(x => new { x.UserId, x.PolicyId }).IsUnique();
    }
}
