namespace Review.Domain.Entities;

public class Review
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? FilmId { get; set; }
    public string Film { get; set; }
    public string User { get; set; }
    public string? UserId { get; set; }
    public string Text { get; set; }
    public DateTime Date { get; set; }
}