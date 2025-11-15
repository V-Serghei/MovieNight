namespace Media.Domain.Entities;

public class MediaFile
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string FileName { get; set; } = default!;

    public string ContentType { get; set; } = default!;

    public long Length { get; set; }

    public byte[] Data { get; set; } = default!;

    public DateTimeOffset CreatedAt { get; set; }
}