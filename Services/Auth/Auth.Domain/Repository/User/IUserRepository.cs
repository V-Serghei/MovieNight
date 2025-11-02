namespace Auth.Domain.Repository.User;

public interface IUserRepository
{
    Task<Entities.User?> FindByEmailAsync(string email, CancellationToken ct = default);
    Task AddAsync(Entities.User user, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}