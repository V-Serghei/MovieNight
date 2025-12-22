namespace MovieNight.Gateway.DTO.User;

public record RegisterRequest(string Email, string Password, string ?DisplayName);