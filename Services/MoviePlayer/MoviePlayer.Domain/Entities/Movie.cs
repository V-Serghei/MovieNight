using System.Text.Json.Serialization;
using MoviePlayer.Domain.Enums;

namespace MoviePlayer.Domain.Entities;

public class Movie
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = default!;
    public MovieCategory Category { get; set; }
    public string PosterImage { get; set; } = default!;
    public string Quote { get; set; } = default!;
    public string Description { get; set; } = default!;
    public int ProductionYear { get; set; } = default!;
    public string Country { get; set; } = "Other";
    public string Director { get; set; } = default!;
    public string Duration { get; set; }
    public float MovieNightGrade { get; set; }
    public string Certificate { get; set; } = default!;
    public string ProductionCompany { get; set; } = default!;
    public string Budget { get; set; } = default!;
    public string GrossWorldwide { get; set; } = default!;
    public string Language { get; set; } = default!;
    public List<string> Genre { get; set; } = new();
}