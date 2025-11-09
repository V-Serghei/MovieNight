using Access.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Access.Infrastructure.Configurations;

public class PolicyConfiguration : IEntityTypeConfiguration<Policy>
{
    public void Configure(EntityTypeBuilder<Policy> e)
    {
        e.HasKey(x => x.Id);
        e.Property(x => x.Resource).HasMaxLength(400).IsRequired();
        e.Property(x => x.Method).HasMaxLength(50);
        e.Property(x => x.Action).HasMaxLength(100);
        e.Property(x => x.ConditionJson).HasColumnType("nvarchar(max)");
        e.Property(x => x.Description).HasMaxLength(400);
    }
}
