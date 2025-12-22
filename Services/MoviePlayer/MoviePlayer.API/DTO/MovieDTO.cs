using MoviePlayer.Domain.Enums;

namespace MoviePlayer.API.DTO;

public record MovieDTO
{
    public string Title { get; init; } = default!;
    public string PosterImage { get; init; } = "";
    public MovieCategory Category { get; init; }

    public string Quote { get; init; } = "";
    public string Description { get; init; } = "";
    public int ProductionYear { get; init; }

    public string Country { get; init; } = "";
    public string Director { get; init; } = "";
    public string Duration { get; init; } = "";
    public string Certificate { get; init; } = "";
    public string ProductionCompany { get; init; } = "";
    public string Budget { get; init; } = "";
    public string GrossWorldwide { get; init; } = "";
    public string Language { get; init; } = "";

    public List<string> Genre { get; init; } = new();

    public List<MovieCardDTO> Cards { get; init; } = new();
    public List<MovieFactDTO> Facts { get; init; } = new();
}