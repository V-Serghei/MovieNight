using Auth.Domain.Entities;

namespace Auth.Domain.Repository.Tokens;

public interface IRefreshTokensRepository
{
    Task<RefreshToken?> FindByHashAsync(string tokenHash, CancellationToken ct = default);
    Task AddAsync(RefreshToken token, CancellationToken ct = default);
    Task RevokeAsync(Guid id, CancellationToken ct = default);
    Task RevokeAllForUserAsync(Guid userId, CancellationToken ct = default); 
    Task SaveChangesAsync(CancellationToken ct = default);
}
