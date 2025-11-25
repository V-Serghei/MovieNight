using System.Text.Json.Serialization;
using MoviePlayer.Domain.Enums;

namespace MoviePlayer.Domain.Entities;

public class Movie
{
    public Guid Id { get; set; }

    public string Title { get; set; } = default!;
    public MovieCategory Category { get; set; }

    public string? PosterImage { get; set; }
    public string? Quote { get; set; }
    public string? Description { get; set; }

    public int ProductionYear { get; set; }
    public string? Country { get; set; }
    public string? Language { get; set; }

    public string? Director { get; set; }
    public string? Duration { get; set; }
    public string? Certificate { get; set; }
    public string? ProductionCompany { get; set; }
    public string? Budget { get; set; }
    public string? GrossWorldwide { get; set; }

    public string? Location { get; set; }

    public ICollection<Genre> Genres { get; set; } = new List<Genre>();
    public ICollection<MovieCard> MovieCards { get; set; } = new List<MovieCard>();
    public ICollection<MovieFact> InterestingFacts { get; set; } = new List<MovieFact>();

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}