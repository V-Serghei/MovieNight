using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Users.API.DTO;
using Users.Core.Entities;
using Users.Core.Repositories;
using Users.Infrastructure.Data;

namespace Users.API.Endpoints;

public static class UserProfileEndpoints
{
    public static IEndpointRouteBuilder MapUserProfileEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/users").WithTags("Users Profile");

        // GET /users/{id}/profile
        g.MapGet("/{id:guid}/profile", GetProfileByUserId);

        // PUT /users/{id}/profile
        g.MapPut("/{id:guid}/profile", UpsertProfileByUserId);

        return app;
    }

    private static async Task<IResult> GetProfileByUserId(
        Guid id,
        UsersDbContext db,
        CancellationToken ct)
    {
        var profile = await db.UserProfiles
            .AsNoTracking()
            .SingleOrDefaultAsync(p => p.UserId == id, ct);

        if (profile is null)
            return Results.NotFound();

        return Results.Ok(UserProfileDto.FromEntity(profile));
    }

    private static async Task<IResult> UpsertProfileByUserId(
        Guid id,
        [FromBody] UserProfileUpdateDto dto,
        UsersDbContext db,
        CancellationToken ct)
    {
        var user = await db.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Id == id, ct);

        if (user is null)
            return Results.NotFound(new { error = "User not found" });

        var profile = await db.UserProfiles
            .SingleOrDefaultAsync(p => p.UserId == id, ct);

        if (profile is null)
        {
            profile = new UserProfile
            {
                UserId = id
            };
            db.UserProfiles.Add(profile);
        }

        profile.UserName = dto.UserName;
        profile.FirstName = dto.FirstName;
        profile.LastName = dto.LastName;
        profile.AboutMe = dto.AboutMe;
        profile.Quote = dto.Quote;
        profile.PhoneNumber = dto.PhoneNumber;
        profile.Gender = dto.Gender;
        profile.DateOfBirth = dto.DateOfBirth;
        profile.Country = dto.Country;
        profile.Facebook = dto.Facebook;
        profile.Twitter = dto.Twitter;
        profile.Instagram = dto.Instagram;
        profile.GitHub = dto.GitHub;
        profile.PersonalInfoFriendsOnly = dto.PersonalInfoFriendsOnly;
        profile.ShowOnlyBasicInfo = dto.ShowOnlyBasicInfo;
        profile.HideBrowsingHistory = dto.HideBrowsingHistory;
        profile.HideGrades = dto.HideGrades;
        profile.AvatarMediaId = dto.AvatarMediaId;

        await db.SaveChangesAsync(ct);

        return Results.Ok(UserProfileDto.FromEntity(profile));
    }
}