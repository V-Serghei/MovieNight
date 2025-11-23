namespace MovieNight.Gateway.DTO.Movie;

public record MovieFactRequest(
    string FactName,
    string? Text
);