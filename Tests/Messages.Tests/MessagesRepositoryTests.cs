using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Messages.Domain.Entities;
using Messages.Domain.Reporitory;
using Messages.Infrastructure.Data;
using Messages.Infrastructure.Repositories;
using Xunit;

namespace Messages.Tests;

public class MessagesRepositoryTests
{
    private static MessagesDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<MessagesDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new MessagesDbContext(options);
    }

    [Fact]
    public async Task AddAsync_And_SaveChangesAsync_Should_PersistMessage()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = CreateContext(dbName);

        IMessagesRepository repo = new MessagesRepository(db);

        var msg = new Messages.Domain.Entities.Messages
        {
            Id = Guid.NewGuid(),
            IsChecked = false,
            SenderName = "Alice",
            SenderId = "user-1",
            RecipientName = "Bob",
            RecipientId = "user-2",
            Theme = "Hello",
            Message = "Hi Bob!",
            Date = DateTime.UtcNow,
            IsStarred = false
        };

        // Act
        await repo.AddAsync(msg, CancellationToken.None);
        await repo.SaveChangesAsync(CancellationToken.None);

        // Assert
        var stored = await db.Messages.SingleOrDefaultAsync(m => m.Id == msg.Id);
        stored.Should().NotBeNull();
        stored!.SenderName.Should().Be("Alice");
        stored.RecipientName.Should().Be("Bob");
        stored.Message.Should().Be("Hi Bob!");
    }

    [Fact]
    public async Task FindByIdAsync_Should_ReturnMessage_WhenExists()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = CreateContext(dbName);

        var id = Guid.NewGuid();

        var msg = new Messages.Domain.Entities.Messages
        {
            Id = id,
            IsChecked = false,
            SenderName = "Alice",
            SenderId = "user-1",
            RecipientName = "Bob",
            RecipientId = "user-2",
            Theme = "Test",
            Message = "Content",
            Date = DateTime.UtcNow,
            IsStarred = false
        };

        db.Messages.Add(msg);
        await db.SaveChangesAsync();

        var repo = new MessagesRepository(db);

        // Act
        var result = await repo.FindByIdAsync(id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
        result.SenderId.Should().Be("user-1");
        result.RecipientId.Should().Be("user-2");
    }

    [Fact]
    public async Task FindByIdAsync_Should_ReturnNull_WhenNotExists()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = CreateContext(dbName);

        var repo = new MessagesRepository(db);

        // Act
        var result = await repo.FindByIdAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task FindBySenderIdAsync_Should_ReturnOnlyMessages_FromGivenSender()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = CreateContext(dbName);

        db.Messages.AddRange(
            new Messages.Domain.Entities.Messages
            {
                Id = Guid.NewGuid(),
                SenderId = "sender-1",
                SenderName = "Alice",
                RecipientId = "user-x",
                RecipientName = "X",
                Theme = "T1",
                Message = "Msg1",
                Date = DateTime.UtcNow,
                IsChecked = false,
                IsStarred = false
            },
            new Messages.Domain.Entities.Messages
            {
                Id = Guid.NewGuid(),
                SenderId = "sender-1",
                SenderName = "Alice",
                RecipientId = "user-y",
                RecipientName = "Y",
                Theme = "T2",
                Message = "Msg2",
                Date = DateTime.UtcNow,
                IsChecked = true,
                IsStarred = true
            },
            new Messages.Domain.Entities.Messages
            {
                Id = Guid.NewGuid(),
                SenderId = "sender-2",
                SenderName = "Bob",
                RecipientId = "user-z",
                RecipientName = "Z",
                Theme = "T3",
                Message = "Msg3",
                Date = DateTime.UtcNow,
                IsChecked = false,
                IsStarred = false
            }
        );
        await db.SaveChangesAsync();

        IMessagesRepository repo = new MessagesRepository(db);

        // Act
        var result = await repo.FindBySenderIdAsync("sender-1", CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.All(m => m.SenderId == "sender-1").Should().BeTrue();
    }

    [Fact]
    public async Task FindBySenderIdAsync_Should_ReturnEmptyList_WhenNoMessages()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = CreateContext(dbName);

        IMessagesRepository repo = new MessagesRepository(db);

        // Act
        var result = await repo.FindBySenderIdAsync("sender-unknown", CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task FindByReceiverIdAsync_Should_ReturnOnlyMessages_ForGivenReceiver()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = CreateContext(dbName);

        db.Messages.AddRange(
            new Messages.Domain.Entities.Messages
            {
                Id = Guid.NewGuid(),
                SenderId = "user-a",
                SenderName = "A",
                RecipientId = "receiver-1",
                RecipientName = "Receiver 1",
                Theme = "T1",
                Message = "Msg1",
                Date = DateTime.UtcNow,
                IsChecked = false,
                IsStarred = false
            },
            new Messages.Domain.Entities.Messages
            {
                Id = Guid.NewGuid(),
                SenderId = "user-b",
                SenderName = "B",
                RecipientId = "receiver-1",
                RecipientName = "Receiver 1",
                Theme = "T2",
                Message = "Msg2",
                Date = DateTime.UtcNow,
                IsChecked = true,
                IsStarred = true
            },
            new Messages.Domain.Entities.Messages
            {
                Id = Guid.NewGuid(),
                SenderId = "user-c",
                SenderName = "C",
                RecipientId = "receiver-2",
                RecipientName = "Receiver 2",
                Theme = "T3",
                Message = "Msg3",
                Date = DateTime.UtcNow,
                IsChecked = false,
                IsStarred = false
            }
        );
        await db.SaveChangesAsync();

        var repo = new MessagesRepository(db);

        // Act
        var result = await repo.FindByReceiverIdAsync("receiver-1", CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.All(m => m.RecipientId == "receiver-1").Should().BeTrue();
    }

    [Fact]
    public async Task SaveChangesAsync_Should_PersistChanges_WhenCalledViaInterface()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = CreateContext(dbName);

        IMessagesRepository repo = new MessagesRepository(db);

        var msg = new Messages.Domain.Entities.Messages
        {
            Id = Guid.NewGuid(),
            SenderId = "user-a",
            SenderName = "A",
            RecipientId = "user-b",
            RecipientName = "B",
            Theme = "Theme",
            Message = "Body",
            Date = DateTime.UtcNow,
            IsChecked = false,
            IsStarred = false
        };

        // Act
        await repo.AddAsync(msg, CancellationToken.None);
        await repo.SaveChangesAsync(CancellationToken.None);

        // Assert
        var stored = await db.Messages.SingleOrDefaultAsync(m => m.Id == msg.Id);
        stored.Should().NotBeNull();
        stored!.Message.Should().Be("Body");
    }
}
