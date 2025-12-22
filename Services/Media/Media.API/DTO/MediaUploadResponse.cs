namespace Media.API.DTO;

public record MediaUploadResponse(
    Guid Id,
    string FileName,
    string ContentType,
    long Length
);

