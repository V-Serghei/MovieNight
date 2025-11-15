namespace Media.API.DTO;

public record MediaInfoResponse(
    Guid Id,
    string FileName,
    string ContentType,
    long Length,
    DateTimeOffset CreatedAt
);