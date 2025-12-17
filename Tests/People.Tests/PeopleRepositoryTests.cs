using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using People.Domain.Entity;
using People.Infrastructure.Data;
using People.Infrastructure.Repositories;
using Xunit;

namespace People.Tests;

public class PeopleRepositoryTests
{
    private static PeopleDbContext CreateContext()
        => PeopleDbContextFactory.CreateContext(Guid.NewGuid().ToString());

    // ---------- Person ----------

    [Fact]
    public async Task AddPersonAsync_Should_SavePerson_AndReturnIt()
    {
        // Arrange
        await using var db = CreateContext();
        var repo = new PeopleRepository(db);

        var person = new Person
        {
            Id = Guid.NewGuid(),
            FullName = "Keanu Reeves",
            KnownForDepartment = "Acting",
            Country = "Canada",
            Bio = "Neo, John Wick",
            BirthDate = new DateTime(1964, 9, 2)
        };

        // Act
        var result = await repo.AddPersonAsync(person, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(person.Id);

        var stored = await db.People.SingleOrDefaultAsync(p => p.Id == person.Id);
        stored.Should().NotBeNull();
        stored!.FullName.Should().Be("Keanu Reeves");
        stored.KnownForDepartment.Should().Be("Acting");
        stored.Country.Should().Be("Canada");
        stored.Bio.Should().Be("Neo, John Wick");
        stored.BirthDate.Should().Be(person.BirthDate);
    }

    [Fact]
    public async Task GetPersonByIdAsync_Should_ReturnPerson_WhenExists()
    {
        // Arrange
        await using var db = CreateContext();
        var repo = new PeopleRepository(db);

        var id = Guid.NewGuid();
        var person = new Person
        {
            Id = id,
            FullName = "Christopher Nolan",
            KnownForDepartment = "Directing"
        };

        db.People.Add(person);
        await db.SaveChangesAsync();

        // Act
        var found = await repo.GetPersonByIdAsync(id, CancellationToken.None);

        // Assert
        found.Should().NotBeNull();
        found!.Id.Should().Be(id);
        found.FullName.Should().Be("Christopher Nolan");
    }

    [Fact]
    public async Task GetPersonByIdAsync_Should_ReturnNull_WhenNotExists()
    {
        // Arrange
        await using var db = CreateContext();
        var repo = new PeopleRepository(db);

        // Act
        var found = await repo.GetPersonByIdAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        found.Should().BeNull();
    }

    [Fact]
    public async Task PersonExistsAsync_Should_ReturnTrue_WhenPersonExists()
    {
        // Arrange
        await using var db = CreateContext();
        var repo = new PeopleRepository(db);

        var id = Guid.NewGuid();
        var person = new Person
        {
            Id = id,
            FullName = "Hans Zimmer"
        };

        db.People.Add(person);
        await db.SaveChangesAsync();

        // Act
        var exists = await repo.PersonExistsAsync(id, CancellationToken.None);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task PersonExistsAsync_Should_ReturnFalse_WhenPersonDoesNotExist()
    {
        // Arrange
        await using var db = CreateContext();
        var repo = new PeopleRepository(db);

        // Act
        var exists = await repo.PersonExistsAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task SearchPeopleByNameAsync_Should_ReturnEmpty_WhenNameIsWhitespace()
    {
        // Arrange
        await using var db = CreateContext();
        var repo = new PeopleRepository(db);

        db.People.Add(new Person
        {
            Id = Guid.NewGuid(),
            FullName = "Keanu Reeves"
        });
        await db.SaveChangesAsync();

        // Act
        var result = await repo.SearchPeopleByNameAsync("   ", 10, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchPeopleByNameAsync_Should_FindBySubstring_AndRespectTake_AndOrderByName()
    {
        // Arrange
        await using var db = CreateContext();
        var repo = new PeopleRepository(db);

        db.People.AddRange(
            new Person
            {
                Id = Guid.NewGuid(),
                FullName = "Keanu Reeves"
            },
            new Person
            {
                Id = Guid.NewGuid(),
                FullName = "Keira Knightley"
            },
            new Person
            {
                Id = Guid.NewGuid(),
                FullName = "Tom Cruise"
            }
        );
        await db.SaveChangesAsync();

        // Act
        var result = await repo.SearchPeopleByNameAsync("Ke", take: 5, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        // сортировка по FullName по возрастанию
        result.Select(p => p.FullName).Should().Equal("Keanu Reeves", "Keira Knightley");

        // проверим ограничение take
        var limited = await repo.SearchPeopleByNameAsync("Ke", take: 1, CancellationToken.None);
        limited.Should().HaveCount(1);
    }

    // ---------- MovieCredit ----------

    [Fact]
    public async Task AddMovieCreditAsync_Should_SetOrder_WhenOrderIsZero_AndNoExistingCredits()
    {
        // Arrange
        await using var db = CreateContext();
        var repo = new PeopleRepository(db);

        var movieId = Guid.NewGuid();
        var personId = Guid.NewGuid();

        var credit = new MovieCredit
        {
            Id = Guid.NewGuid(),
            MovieId = movieId,
            PersonId = personId,
            Role = CreditRole.Actor,
            Order = 0,
            CharacterName = "Neo"
        };

        // Act
        var result = await repo.AddMovieCreditAsync(credit, CancellationToken.None);

        // Assert
        result.Order.Should().Be(1);

        var stored = await db.MovieCredits.SingleAsync(c => c.Id == credit.Id);
        stored.Order.Should().Be(1);
    }

    [Fact]
    public async Task AddMovieCreditAsync_Should_SetNextOrder_BasedOnExistingCredits_WithSameMovieAndRole()
    {
        // Arrange
        await using var db = CreateContext();
        var repo = new PeopleRepository(db);

        var movieId = Guid.NewGuid();

        var p1 = Guid.NewGuid();
        var p2 = Guid.NewGuid();
        var p3 = Guid.NewGuid();

        db.People.AddRange(
            new Person { Id = p1, FullName = "Actor 1" },
            new Person { Id = p2, FullName = "Actor 2" },
            new Person { Id = p3, FullName = "Actor 3" }
        );

        db.MovieCredits.AddRange(
            new MovieCredit
            {
                Id = Guid.NewGuid(),
                MovieId = movieId,
                PersonId = p1,
                Role = CreditRole.Actor,
                Order = 1,
                CharacterName = "Char1"
            },
            new MovieCredit
            {
                Id = Guid.NewGuid(),
                MovieId = movieId,
                PersonId = p2,
                Role = CreditRole.Actor,
                Order = 2,
                CharacterName = "Char2"
            },
            // другой фильм – не должен влиять на Order
            new MovieCredit
            {
                Id = Guid.NewGuid(),
                MovieId = Guid.NewGuid(),
                PersonId = p3,
                Role = CreditRole.Actor,
                Order = 10,
                CharacterName = "OtherMovie"
            },
            // другая роль в том же фильме – тоже не влияет
            new MovieCredit
            {
                Id = Guid.NewGuid(),
                MovieId = movieId,
                PersonId = p3,
                Role = CreditRole.Director,
                Order = 5,
                CharacterName = "N/A"
            }
        );

        await db.SaveChangesAsync();

        var newCredit = new MovieCredit
        {
            Id = Guid.NewGuid(),
            MovieId = movieId,
            PersonId = Guid.NewGuid(),
            Role = CreditRole.Actor,
            Order = 0,
            CharacterName = "Char3"
        };

        // Act
        var result = await repo.AddMovieCreditAsync(newCredit, CancellationToken.None);

        // Assert
        result.Order.Should().Be(3); // после max(1,2) по Actor/этот фильм

        var stored = await db.MovieCredits.SingleAsync(c => c.Id == newCredit.Id);
        stored.Order.Should().Be(3);
    }

    [Fact]
    public async Task AddMovieCreditAsync_Should_NotChangeOrder_WhenOrderAlreadySet()
    {
        // Arrange
        await using var db = CreateContext();
        var repo = new PeopleRepository(db);

        var movieId = Guid.NewGuid();
        var personId = Guid.NewGuid();

        var credit = new MovieCredit
        {
            Id = Guid.NewGuid(),
            MovieId = movieId,
            PersonId = personId,
            Role = CreditRole.Actor,
            Order = 7,
            CharacterName = "Some Char"
        };

        // Act
        var result = await repo.AddMovieCreditAsync(credit, CancellationToken.None);

        // Assert
        result.Order.Should().Be(7);

        var stored = await db.MovieCredits.SingleAsync(c => c.Id == credit.Id);
        stored.Order.Should().Be(7);
    }

    [Fact]
    public async Task GetCreditsForMovieAsync_Should_ReturnCreditsJoinedWithPeople_OrderedByOrder()
    {
        // Arrange
        await using var db = CreateContext();
        var repo = new PeopleRepository(db);

        var movieId = Guid.NewGuid();
        var otherMovieId = Guid.NewGuid();

        var p1 = new Person { Id = Guid.NewGuid(), FullName = "Actor 1" };
        var p2 = new Person { Id = Guid.NewGuid(), FullName = "Actor 2" };
        var p3 = new Person { Id = Guid.NewGuid(), FullName = "Actor 3" };

        db.People.AddRange(p1, p2, p3);

        var c1 = new MovieCredit
        {
            Id = Guid.NewGuid(),
            MovieId = movieId,
            PersonId = p2.Id,
            Role = CreditRole.Actor,
            Order = 2,
            CharacterName = "Char2"
        };

        var c2 = new MovieCredit
        {
            Id = Guid.NewGuid(),
            MovieId = movieId,
            PersonId = p1.Id,
            Role = CreditRole.Actor,
            Order = 1,
            CharacterName = "Char1"
        };

        var cOther = new MovieCredit
        {
            Id = Guid.NewGuid(),
            MovieId = otherMovieId,
            PersonId = p3.Id,
            Role = CreditRole.Actor,
            Order = 1,
            CharacterName = "OtherMovie"
        };

        db.MovieCredits.AddRange(c1, c2, cOther);
        await db.SaveChangesAsync();

        // Act
        var result = await repo.GetCreditsForMovieAsync(movieId, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);

        // порядок по Order: сначала c2 (Order=1), затем c1 (Order=2)
        result[0].Credit.Id.Should().Be(c2.Id);
        result[0].Person.Id.Should().Be(p1.Id);

        result[1].Credit.Id.Should().Be(c1.Id);
        result[1].Person.Id.Should().Be(p2.Id);

        // ни одного кредита от другого фильма
        result.Select(x => x.Credit.MovieId).Distinct().Should().ContainSingle().Which.Should().Be(movieId);
    }
}
