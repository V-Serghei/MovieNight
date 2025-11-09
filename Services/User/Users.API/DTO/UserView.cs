using Users.Core.Entities;

namespace Users.API.DTO;
public sealed record UserView(Guid Id, string Email, string? DisplayName, bool IsActive)
{
    public static UserView From(User u) => new(u.Id, u.Email!, u.DisplayName, u.IsActive);
}