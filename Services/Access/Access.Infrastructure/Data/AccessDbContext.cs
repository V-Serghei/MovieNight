using Access.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Access.Infrastructure.Data;

public class AccessDbContext(DbContextOptions<AccessDbContext> options) : DbContext(options)
{
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Policy> Policies => Set<Policy>();
    public DbSet<RolePolicy> RolePolicies => Set<RolePolicy>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<UserPolicy> UserPolicies => Set<UserPolicy>();

    protected override void OnModelCreating(ModelBuilder b)
        => b.ApplyConfigurationsFromAssembly(typeof(AccessDbContext).Assembly);
}
