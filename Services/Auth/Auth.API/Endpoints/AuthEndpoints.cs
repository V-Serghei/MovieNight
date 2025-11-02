using Auth.API.DTO;
using Auth.Domain.Repository.User;
using Auth.Domain.Entities;
using Auth.Core.Security;
using Microsoft.AspNetCore.Routing;

namespace Auth.API.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder routes)
    {
        var g = routes.MapGroup("/auth").WithTags("Auth");


        g.MapPost("/register", async (RegisterDto dto, IUserRepository users, JwtTokenService jwt, CancellationToken ct) =>
        {
            var exists = await users.FindByEmailAsync(dto.Email, ct);
            if (exists is not null) return Results.Conflict(new { error = "Email already in use" });

            var (hash, salt) = PasswordHasher.Hash(dto.Password);

            var user = new User
            {
                Email        = dto.Email,
                PasswordHash = hash,
                PasswordSalt = salt,
                DisplayName  = dto.DisplayName
            };

            await users.AddAsync(user, ct);
            await users.SaveChangesAsync(ct);

            var (token, exp) = jwt.CreateAccessToken(user.Id, user.Email);
            return Results.Created($"/auth/users/{user.Id}", new
            {
                user = new { user.Id, user.Email, user.DisplayName },
                token,
                exp
            });
        });

        g.MapPost("/login", async (LoginDto dto, IUserRepository users, JwtTokenService jwt, CancellationToken ct) =>
        {
            var user = await users.FindByEmailAsync(dto.Email, ct);
            if (user is null) return Results.Unauthorized();

            var ok = PasswordHasher.Verify(dto.Password, user.PasswordHash, user.PasswordSalt);
            if (!ok) return Results.Unauthorized();

            var (token, exp) = jwt.CreateAccessToken(user.Id, user.Email);
            return Results.Ok(new
            {
                user = new { user.Id, user.Email, user.DisplayName },
                token,
                exp
            });
        });

        return routes;
    }
}
