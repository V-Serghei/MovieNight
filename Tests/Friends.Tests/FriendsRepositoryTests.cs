using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Friends.Domain.Entities;
using Friends.Infrastructure;
using Friends.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Friends.Tests;

public class FriendsRepositoryTests
{
    private static FriendsDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<FriendsDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new FriendsDbContext(options);
    }

    [Fact]
    public async Task FindFriendsByUserIdAsync_Should_ReturnOnlyFriendsOfGivenUser()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var db = CreateContext(dbName);

        var user1 = "user-1";
        var user2 = "user-2";

        db.Friends.AddRange(
            new Friends.Domain.Entities.Friends
            {
                IdUser = user1,
                IdFriend = "friend-a",
                KindOfFriendship = (Friends.Domain.Enums.Friendship)1
            },
            new Friends.Domain.Entities.Friends
            {
                IdUser = user1,
                IdFriend = "friend-b",
                KindOfFriendship = (Friends.Domain.Enums.Friendship)2
            },
            new Friends.Domain.Entities.Friends
            {
                IdUser = user2,
                IdFriend = "friend-x",
                KindOfFriendship = (Friends.Domain.Enums.Friendship)1
            }
        );
        await db.SaveChangesAsync();

        var repo = new FriendsRepository(db);

        var result = await repo.FindFriendsByUserIdAsync(user1);

        result.Should().HaveCount(2);
        result.Should().OnlyContain(f => f.IdUser == user1);
        result.Select(f => f.IdFriend).Should().BeEquivalentTo(new[] { "friend-a", "friend-b" });
    }

    [Fact]
    public async Task FindFriendsByUserIdAsync_Should_ReturnEmptyList_WhenUserHasNoFriends()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var db = CreateContext(dbName);

        var repo = new FriendsRepository(db);

        var result = await repo.FindFriendsByUserIdAsync("unknown-user");

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task AddAsync_And_SaveChangesAsync_Should_PersistFriendship()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var db = CreateContext(dbName);

        var repo = new FriendsRepository(db);

        var friendship = new Friends.Domain.Entities.Friends
        {
            IdUser = "user-1",
            IdFriend = "friend-1",
            KindOfFriendship = (Friends.Domain.Enums.Friendship)1
        };

        await repo.AddAsync(friendship, CancellationToken.None);
        await repo.SaveChangesAsync(CancellationToken.None);

        var all = await db.Friends.ToListAsync();
        all.Should().HaveCount(1);

        var stored = all.Single();
        stored.IdUser.Should().Be(friendship.IdUser);
        stored.IdFriend.Should().Be(friendship.IdFriend);
        stored.KindOfFriendship.Should().Be(friendship.KindOfFriendship);
    }

    [Fact]
    public async Task AddAsync_Should_NotSaveWithout_SaveChangesAsync()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var db = CreateContext(dbName);

        var repo = new FriendsRepository(db);

        var friendship = new Friends.Domain.Entities.Friends
        {
            IdUser = "user-1",
            IdFriend = "friend-2",
            KindOfFriendship = (Friends.Domain.Enums.Friendship)2
        };

        await repo.AddAsync(friendship, CancellationToken.None);

        var all = await db.Friends.ToListAsync();
        all.Should().BeEmpty("без SaveChangesAsync EF не должен фиксировать изменения в базе (InMemory)");
    }
}
