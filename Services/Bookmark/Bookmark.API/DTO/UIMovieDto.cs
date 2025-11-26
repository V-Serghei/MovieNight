namespace Bookmark.API.DTO;

public record UIMovieDto(
    Guid Id,
    string Title,
    int Year,
    string? Duration,
    string? PosterImage
);