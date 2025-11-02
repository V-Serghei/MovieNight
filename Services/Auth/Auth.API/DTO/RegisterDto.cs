namespace Auth.API.DTO;

public record RegisterDto(string Email, string Password, string? DisplayName);