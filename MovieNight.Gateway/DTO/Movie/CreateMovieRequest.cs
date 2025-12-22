using MovieNight.Gateway.Enums;

namespace MovieNight.Gateway.DTO.Movie;

public record CreateMovieRequest(
    string Title,
    MovieCategory Category,
    string PosterImage,
    string Quote,
    string Description,
    int ProductionYear,
    string Country,
    string Director,
    string Duration,
    string Certificate,
    string ProductionCompany,
    string Budget,
    string GrossWorldwide,
    string Language,
    List<string> Genre,
    List<MovieCardRequest> Cards,
    List<MovieFactRequest> Facts
);