using MovieNight.Gateway.Enums;

namespace MovieNight.Gateway.DTO.Movie;

public record CreateMovieRequest(
    string Title,
    MovieCategory Category,
    string PosterImage,
    string Quote,
    string Description,
    DateTime ProductionYear,
    string ProductionYearS,
    string Country,
    string Director,
    DateTime Duration,
    string Certificate,
    string ProductionCompany,
    string Budget,
    List<string> Genre
);