namespace Users.Core.Entities;

public class User
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string? Email { get; set; }
    public string EmailNormalized { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public string PasswordSalt { get; set; } = default!;
    public string? DisplayName { get; set; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LastLoginAt { get; set; }
    public bool IsActive { get; set; } = true;

    public UserProfile? Profile { get; set; }
}