using Users.Core.Entities;

namespace Users.Core.Repositories;

public interface IUserRepository
{
    Task<bool> ExistsByEmailAsync(string emailNorm, CancellationToken ct = default);
    Task<User?> FindByEmailAsync(string emailNorm, CancellationToken ct = default);
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<User>?> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}