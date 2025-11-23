namespace People.Domain.Entity;

public class MovieCredit
{
    public Guid Id { get; set; }
    public Guid MovieId { get; set; }
    public Guid PersonId { get; set; }

    public CreditRole Role { get; set; }
    public string? CharacterName { get; set; } 
    public int Order { get; set; } 
}