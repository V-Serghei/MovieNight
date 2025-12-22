using System.Text.Json.Serialization;

namespace MoviePlayer.Domain.Entities;

public class MovieCard
{
    public Guid Id { get; set; }

    public Guid MovieId { get; set; }

    [JsonIgnore]
    public Movie Movie { get; set; } = default!;

    public string ImageUrl { get; set; } = default!;
    public string? Title { get; set; }
    public string? Description { get; set; }
}