using System.Text.Json.Serialization;

namespace MoviePlayer.Domain.Entities;

public class Genre
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;

    [JsonIgnore]
    public ICollection<Movie> Movies { get; set; } = new List<Movie>();
}