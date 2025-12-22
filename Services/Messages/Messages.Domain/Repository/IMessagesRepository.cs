namespace Messages.Domain.Reporitory;

public interface IMessagesRepository
{
    Task<Entities.Messages?> FindByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Entities.Messages>> FindBySenderIdAsync(string senderId, CancellationToken ct = default);
    Task AddAsync(Entities.Messages messages, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
    Task<List<Entities.Messages>> FindByReceiverIdAsync(string receiverId, CancellationToken ct = default);
}