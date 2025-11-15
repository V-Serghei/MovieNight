namespace Messages.Domain.Entities;

public class Messages
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public bool IsChecked { get; set; }
    public string SenderName { get; set; }
    public string SenderId { get; set; }
    public string RecipientName { get; set; }
    public string RecipientId { get; set; }
    public string Theme { get; set; }
    public string Message { get; set; }
    public DateTime Date { get; set; }
    public bool IsStarred { get; set; }
}