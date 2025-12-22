using Microsoft.AspNetCore.Mvc;
using Users.API.DTO;
using Users.Core.Entities;
using Users.Core.Repositories;
using Users.Core.Security;


namespace Users.API.Endpoints;


public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/users").WithTags("Users");


        g.MapPost("/register", async ([FromBody] RegisterDto dto, IUserRepository repo, CancellationToken ct) =>
        {
            var emailNorm = dto.Email.Trim().ToLowerInvariant();
            if (await repo.ExistsByEmailAsync(emailNorm, ct))
                return Results.Conflict(new { error = "Email already in use" });


            var (hash, salt) = PasswordHasher.Hash(dto.Password);
            var user = new User
            {
                Email = dto.Email.Trim(),
                EmailNormalized = emailNorm,
                PasswordHash = hash,
                PasswordSalt = salt,
                DisplayName = dto.DisplayName,
                IsActive = true
            };
            await repo.AddAsync(user, ct);
            await repo.SaveChangesAsync(ct);
            return Results.Created($"/users/{user.Id}", UserView.From(user));
        });


        g.MapPost("/verify-credentials", async ([FromBody] VerifyCredentialsDto dto, IUserRepository repo, CancellationToken ct) =>
        {
            var user = await repo.FindByEmailAsync(dto.Email.Trim().ToLowerInvariant(), ct);
            if (user is null || !user.IsActive)
                return Results.Ok(new { ok = false });
            var ok = PasswordHasher.Verify(dto.Password, user.PasswordHash, user.PasswordSalt);
            if (!ok) return Results.Ok(new { ok = false });
            return Results.Ok(new { ok = true, user = UserView.From(user) });
        });


        g.MapGet("/{id:guid}", async (Guid id, IUserRepository repo, CancellationToken ct) =>
        {
            var user = await repo.GetByIdAsync(id, ct);
            return user is null ? Results.NotFound() : Results.Ok(UserView.From(user));
        });

        g.MapGet("/", async (IUserRepository repo, CancellationToken ct) =>
        {
            var user = await repo.GetAllAsync(ct);
            return user is null ? Results.NotFound() : Results.Ok(user.Select(u=>UserView.From(u)));
        });

        return app;
    }
}