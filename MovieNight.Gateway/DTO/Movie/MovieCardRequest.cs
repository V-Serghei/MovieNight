namespace MovieNight.Gateway.DTO.Movie;

public record MovieCardRequest(
    string Title,
    string ImageUrl,
    string Description
);