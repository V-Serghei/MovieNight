using Microsoft.AspNetCore.Mvc;
using People.API.DTO;
using People.Domain.Entity;
using People.Domain.Repository;

namespace People.API.Endpoints;

public static class PeopleEndpoints
{
    public static IEndpointRouteBuilder MapPeopleEndpoints(this IEndpointRouteBuilder routes)
    {
        var g = routes.MapGroup("/people").WithTags("People");

        g.MapPost("/", CreatePerson).WithOpenApi();
        g.MapGet("/{id:guid}", GetPersonById).WithOpenApi();
        g.MapGet("/search", SearchPeople).WithOpenApi();

        g.MapPost("/credits", CreateCredit).WithOpenApi();
        g.MapGet("/movies/{movieId:guid}/credits", GetCreditsForMovie).WithOpenApi();

        return routes;
    }

    // POST /people
    private static async Task<IResult> CreatePerson(
        [FromBody] PersonCreateRequest req,
        IPeopleRepository repo,
        CancellationToken ct)
    {
        var person = new Person
        {
            Id = Guid.NewGuid(),
            FullName = req.FullName,
            KnownForDepartment = req.KnownForDepartment,
            BirthDate = req.BirthDate,
            Country = req.Country,
            Bio = req.Bio,
            ProfileImageId = req.ProfileImageId
        };

        var saved = await repo.AddPersonAsync(person, ct);

        var dto = new PersonResponse(
            saved.Id,
            saved.FullName,
            saved.KnownForDepartment,
            saved.BirthDate,
            saved.Country,
            saved.Bio,
            saved.ProfileImageId
        );

        return Results.Created($"/people/{saved.Id}", dto);
    }

    // GET /people/{id}
    private static async Task<IResult> GetPersonById(
        Guid id,
        IPeopleRepository repo,
        CancellationToken ct)
    {
        var p = await repo.GetPersonByIdAsync(id, ct);
        if (p is null) return Results.NotFound();

        var dto = new PersonResponse(
            p.Id,
            p.FullName,
            p.KnownForDepartment,
            p.BirthDate,
            p.Country,
            p.Bio,
            p.ProfileImageId
        );

        return Results.Ok(dto);
    }

    // GET /people/search?name=...
    private static async Task<IResult> SearchPeople(
        [FromQuery] string name,
        IPeopleRepository repo,
        CancellationToken ct)
    {
        name = name.Trim();
        if (string.IsNullOrWhiteSpace(name))
            return Results.BadRequest("name is required");

        var people = await repo.SearchPeopleByNameAsync(name, 50, ct);

        var dtos = people
            .Select(p => new PersonResponse(
                p.Id,
                p.FullName,
                p.KnownForDepartment,
                p.BirthDate,
                p.Country,
                p.Bio,
                p.ProfileImageId))
            .ToList();

        return Results.Ok(dtos);
    }

    // POST /people/credits
    private static async Task<IResult> CreateCredit(
        [FromBody] MovieCreditCreateRequest req,
        IPeopleRepository repo,
        CancellationToken ct)
    {
        var personExists = await repo.PersonExistsAsync(req.PersonId, ct);
        if (!personExists)
            return Results.BadRequest("Person does not exist");

        var credit = new MovieCredit
        {
            Id = Guid.NewGuid(),
            MovieId = req.MovieId,
            PersonId = req.PersonId,
            Role = req.Role,
            CharacterName = req.CharacterName,
            Order = req.Order ?? 0
        };

        var saved = await repo.AddMovieCreditAsync(credit, ct);

        var person = await repo.GetPersonByIdAsync(saved.PersonId, ct)
                     ?? throw new InvalidOperationException("Person not found after create");

        var dto = new MovieCreditResponse(
            saved.Id,
            saved.MovieId,
            saved.PersonId,
            saved.Role,
            saved.CharacterName,
            saved.Order,
            person.FullName,
            person.ProfileImageId
        );

        return Results.Created($"/people/credits/{saved.Id}", dto);
    }

    // GET /people/movies/{movieId}/credits
    private static async Task<IResult> GetCreditsForMovie(
        Guid movieId,
        IPeopleRepository repo,
        CancellationToken ct)
    {
        var data = await repo.GetCreditsForMovieAsync(movieId, ct);

        var dtos = data
            .Select(x => new MovieCreditResponse(
                x.Credit.Id,
                x.Credit.MovieId,
                x.Credit.PersonId,
                x.Credit.Role,
                x.Credit.CharacterName,
                x.Credit.Order,
                x.Person.FullName,
                x.Person.ProfileImageId        ))
            .ToList();

        return Results.Ok(dtos);
    }
}