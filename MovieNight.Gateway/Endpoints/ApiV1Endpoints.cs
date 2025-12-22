using Microsoft.AspNetCore.Routing;

namespace MovieNight.Gateway.Endpoints;

public static class ApiV1Endpoints
{
    public static IEndpointRouteBuilder MapApiV1(this IEndpointRouteBuilder routes)
    {
        var api = routes.MapGroup("/api/v1").WithTags("API v1");

        //  GET /api/v1/movies/42
        api.MapGet("/movies/{id:int}", (int id) =>
            Results.Ok(new { id, title = "Stub movie", year = 2025 }));

        //POST /api/v1/movies 
        api.MapPost("/movies", (MovieCreate dto) =>
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["title"] = new[] { "Title is required." }
                }, statusCode: 400);

            //  id
            return Results.Created($"/api/v1/movies/1", new { id = 1, dto.Title, dto.Year });
        });

        return routes;
    }

    public record MovieCreate(string Title, int? Year);
}