namespace MovieNight.Gateway.DTO.Messages;

public record MessagesRequest(bool IsChecked, string SenderName, string SenderId, string RecipientName, string RecipientId,
    string Theme, string Message, DateTime Date, bool IsStarred);