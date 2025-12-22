using Media.Domain.Entities;

namespace Media.Core.Repositories;

public interface IMediaRepository
{
    Task<MediaFile?> GetAsync(Guid id, CancellationToken ct = default);
    Task<MediaFile> AddAsync(MediaFile file, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}