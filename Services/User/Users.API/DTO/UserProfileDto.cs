using Users.Core.Entities;

namespace Users.API.DTO;

public sealed record UserProfileDto(
    Guid UserId,
    string? UserName,
    string? FirstName,
    string? LastName,
    string? AboutMe,
    string? Quote,
    string? PhoneNumber,
    string? Gender,
    DateTimeOffset? DateOfBirth,
    string? Country,
    string? Facebook,
    string? Twitter,
    string? Instagram,
    string? GitHub,
    bool PersonalInfoFriendsOnly,
    bool ShowOnlyBasicInfo,
    bool HideBrowsingHistory,
    bool HideGrades,
    string? AvatarMediaId)
{
    public static UserProfileDto FromEntity(UserProfile p) => new(
        p.UserId,
        p.UserName,
        p.FirstName,
        p.LastName,
        p.AboutMe,
        p.Quote,
        p.PhoneNumber,
        p.Gender,
        p.DateOfBirth,
        p.Country,
        p.Facebook,
        p.Twitter,
        p.Instagram,
        p.GitHub,
        p.PersonalInfoFriendsOnly,
        p.ShowOnlyBasicInfo,
        p.HideBrowsingHistory,
        p.HideGrades,
        p.AvatarMediaId
    );
}