using Auth.Domain.Entities;
using Auth.Domain.Repository.Tokens;
using Auth.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Repositories;

public class RefreshTokenRepository(AuthDbContext db) : IRefreshTokensRepository
{
    public Task<RefreshToken?> FindByHashAsync(string tokenHash, CancellationToken ct = default)
        => db.RefreshTokens.SingleOrDefaultAsync(x => x.Token == tokenHash, ct);

    public Task AddAsync(RefreshToken token, CancellationToken ct = default)
        => db.RefreshTokens.AddAsync(token, ct).AsTask();

    public async Task RevokeAsync(Guid id, CancellationToken ct = default)
    {
        var rt = await db.RefreshTokens.FindAsync([id], ct);
        if (rt != null) rt.Revoked = true;
    }

    public Task RevokeAllForUserAsync(Guid userId, CancellationToken ct = default)
        => db.RefreshTokens.Where(x => x.UserId == userId && !x.Revoked)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.Revoked, true), ct);

    public Task SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}