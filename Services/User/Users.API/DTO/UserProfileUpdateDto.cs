namespace Users.API.DTO;

public sealed record UserProfileUpdateDto(
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
    string? AvatarMediaId);