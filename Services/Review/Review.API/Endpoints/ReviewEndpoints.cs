using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Routing;
using Review.Domain.Entities;
using Review.Domain.Repository.Review;
using Review.API.DTO;

namespace Review.API.Endpoints;

public static class ReviewEndpoints
{
    public static IEndpointRouteBuilder MapReviewEndpoints(this IEndpointRouteBuilder routes)
    {
        var g = routes.MapGroup("/review").WithTags("Review");

        g.MapGet("/{filmId}", async (string filmId, IReviewRepository repo, CancellationToken ct) =>
        {
            var reviews = await repo.FindByFilmIdAsync(filmId, ct);
            return Results.Ok(reviews);
        });

        g.MapPost("/", async (HttpContext ctx, ReviewDTO reviewDto, IReviewRepository repo, CancellationToken ct) =>
        {
            var userId = ctx.Request.Headers["X-UserId"].FirstOrDefault();
            var userName = ctx.Request.Headers["X-UserName"].FirstOrDefault();
            
            if (string.IsNullOrWhiteSpace(userId))
            {
                var user = ctx.User;
                userId = user.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? user.FindFirstValue(JwtRegisteredClaimNames.Sub);
            }

            if (string.IsNullOrWhiteSpace(userName))
            {
                var user = ctx.User;
                userName = user.FindFirstValue(ClaimTypes.Name)
                           ?? user.FindFirstValue(ClaimTypes.Email)
                           ?? "Anonymous";
            }

            var review = new Domain.Entities.Review
            {
                Id = Guid.NewGuid(),
                FilmId = reviewDto.FilmId,
                Film  = reviewDto.Film,
                UserId = userId,
                User   = userName ?? "Anonymous",
                Text   = reviewDto.Text,
                Date   = DateTime.UtcNow
            };

            await repo.AddAsync(review, ct);
            await repo.SaveChangesAsync(ct);

            return Results.Created($"/review/{review.Id}", review);
        });

        return routes;
    }
}