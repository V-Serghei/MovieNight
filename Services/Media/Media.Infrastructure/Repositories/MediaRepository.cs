using Media.Core.Repositories;
using Media.Domain.Entities;
using Media.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Media.Infrastructure.Repositories;

public class MediaRepository : IMediaRepository
{
    private readonly MediaDbContext _db;

    public MediaRepository(MediaDbContext db)
    {
        _db = db;
    }

    public async Task<MediaFile?> GetAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.MediaFiles
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<MediaFile> AddAsync(MediaFile file, CancellationToken ct = default)
    {
        await _db.MediaFiles.AddAsync(file, ct);
        await _db.SaveChangesAsync(ct);
        return file;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.MediaFiles.FindAsync(new object[] { id }, ct);
        if (entity is null) return;

        _db.MediaFiles.Remove(entity);
        await _db.SaveChangesAsync(ct);
    }
}