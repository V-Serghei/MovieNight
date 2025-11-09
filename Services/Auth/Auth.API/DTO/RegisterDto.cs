namespace Auth.API.DTO;

public sealed record RegisterDto(string Email, string Password, string? DisplayName, bool TermsAccepted);