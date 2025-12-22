using System.Text.Json.Serialization;

namespace MoviePlayer.Domain.Entities;

public class MovieFact
{
    public Guid Id { get; set; }

    public Guid MovieId { get; set; }

    [JsonIgnore]
    public Movie Movie { get; set; } = default!;

    public string FactName { get; set; } = default!;
    public string? Text { get; set; }
}