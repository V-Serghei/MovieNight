using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using User.Tests;
using Users.Core.Entities;
using Users.Infrastructure.Configurations;
using Users.Infrastructure.Data;
using Xunit;

namespace Users.Tests;

public class UserProfileRepositoryTests
{
    private static UsersDbContext CreateContext()
        => UsersDbContextFactory.Create(Guid.NewGuid().ToString());

    [Fact]
    public async Task GetByUserIdAsync_Should_ReturnProfile_WhenExists()
    {
        await using var db = CreateContext();

        var userId = Guid.NewGuid();

        var user = new Core.Entities.User
        {
            Id = userId,
            Email = "profile@example.com",
            EmailNormalized = "PROFILE@EXAMPLE.COM",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            DisplayName = "ProfileUser"
        };

        var profile = new UserProfile
        {
            UserId = userId,
            User = user,
            UserName = "profile_user",
            FirstName = "John",
            LastName = "Doe",
            AboutMe = "Hello"
        };

        db.Users.Add(user);
        db.UserProfiles.Add(profile);
        await db.SaveChangesAsync();

        var repo = new UserProfileRepository(db);

        var found = await repo.GetByUserIdAsync(userId);

        found.Should().NotBeNull();
        found!.UserId.Should().Be(userId);
        found.UserName.Should().Be("profile_user");
        found.FirstName.Should().Be("John");
        found.LastName.Should().Be("Doe");
    }

    [Fact]
    public async Task GetByUserIdAsync_Should_ReturnNull_WhenNotExists()
    {
        await using var db = CreateContext();
        var repo = new UserProfileRepository(db);

        var found = await repo.GetByUserIdAsync(Guid.NewGuid());

        found.Should().BeNull();
    }

    [Fact]
    public async Task AddOrUpdateAsync_Should_InsertProfile_WhenNotExists()
    {
        await using var db = CreateContext();
        var repo = new UserProfileRepository(db);

        var userId = Guid.NewGuid();
        var user = new Core.Entities.User
        {
            Id = userId,
            Email = "newprofile@example.com",
            EmailNormalized = "NEWPROFILE@EXAMPLE.COM",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            DisplayName = "UserWithProfile"
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var profile = new UserProfile
        {
            UserId = userId,
            User = user,
            UserName = "new_profile",
            FirstName = "Jane",
            LastName = "Smith",
            AboutMe = "Hi there"
        };

        await repo.AddOrUpdateAsync(profile);
        await repo.SaveChangesAsync();

        var stored = await db.UserProfiles.SingleOrDefaultAsync(p => p.UserId == userId);
        stored.Should().NotBeNull();
        stored!.UserName.Should().Be("new_profile");
        stored.FirstName.Should().Be("Jane");
        stored.LastName.Should().Be("Smith");
    }

    [Fact]
    public async Task AddOrUpdateAsync_Should_UpdateProfile_WhenExists()
    {
        await using var db = CreateContext();
        var repo = new UserProfileRepository(db);

        var userId = Guid.NewGuid();
        var user = new Core.Entities.User
        {
            Id = userId,
            Email = "updateprofile@example.com",
            EmailNormalized = "UPDATEPROFILE@EXAMPLE.COM",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            DisplayName = "UserWithProfile"
        };

        var existingProfile = new UserProfile
        {
            UserId = userId,
            User = user,
            UserName = "old_name",
            FirstName = "Old",
            LastName = "Name",
            AboutMe = "Old about"
        };

        db.Users.Add(user);
        db.UserProfiles.Add(existingProfile);
        await db.SaveChangesAsync();

        var updatedProfile = new UserProfile
        {
            UserId = userId,
            User = user, // навигация не важна для SetValues
            UserName = "new_name",
            FirstName = "NewFirst",
            LastName = "NewLast",
            AboutMe = "New about",
            Country = "USA",
            GitHub = "github_user"
        };

        await repo.AddOrUpdateAsync(updatedProfile);
        await repo.SaveChangesAsync();

        var stored = await db.UserProfiles.SingleOrDefaultAsync(p => p.UserId == userId);
        stored.Should().NotBeNull();
        stored!.UserName.Should().Be("new_name");
        stored.FirstName.Should().Be("NewFirst");
        stored.LastName.Should().Be("NewLast");
        stored.AboutMe.Should().Be("New about");
        stored.Country.Should().Be("USA");
        stored.GitHub.Should().Be("github_user");

        // и важно — не появилось второй записи
        var count = await db.UserProfiles.CountAsync();
        count.Should().Be(1);
    }
}
