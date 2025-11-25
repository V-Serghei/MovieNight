namespace Users.Core.Entities;

public class UserProfile
{
    // PK = FK to User
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;

    public string? UserName { get; set; }      
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? AboutMe { get; set; }
    public string? Quote { get; set; }

    public string? PhoneNumber { get; set; }
    public string? Gender { get; set; }         
    public DateTimeOffset? DateOfBirth { get; set; }
    public string? Country { get; set; }

    public string? Facebook { get; set; }
    public string? Twitter { get; set; }
    public string? Instagram { get; set; }
    public string? GitHub { get; set; }

    public bool PersonalInfoFriendsOnly { get; set; } // "Your personal information can only be seen by your friends"
    public bool ShowOnlyBasicInfo { get; set; }       // "Show everyone only basic information about you"
    public bool HideBrowsingHistory { get; set; }     // "Hide your browsing history"
    public bool HideGrades { get; set; }              // "Hide my grades"

    public string? AvatarMediaId { get; set; }
}