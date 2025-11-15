namespace Messages.API.DTO;

public record MessagesDTO(bool IsChecked, string SenderName, string SenderId, string RecipientName, string RecipientId,
    string Theme, string Message, DateTime Date, bool IsStarred);