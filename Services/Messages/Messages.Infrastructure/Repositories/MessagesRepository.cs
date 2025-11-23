using Messages.Domain.Reporitory;
using Messages.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Messages.Infrastructure.Repositories;

public class MessagesRepository(MessagesDbContext db) : IMessagesRepository
{
    public Task<Domain.Entities.Messages?> FindByIdAsync(Guid id, CancellationToken ct = default)
        => db.Messages.SingleOrDefaultAsync(m => m.Id == id, ct);
    
    public Task AddAsync(Domain.Entities.Messages messages, CancellationToken ct = default)
        => db.Messages.AddAsync(messages, ct).AsTask();

    Task IMessagesRepository.SaveChangesAsync(CancellationToken ct)
        => db.SaveChangesAsync(ct);
    
    Task<List<Domain.Entities.Messages>> IMessagesRepository.FindBySenderIdAsync(string senderId, CancellationToken ct)
        => db.Messages.Where(
            m => m.SenderId == senderId).ToListAsync<Domain.Entities.Messages>(ct);
    
    public Task<Domain.Entities.Messages?> FindByReceiverIdAsync(string receiverId, CancellationToken ct = default)
        => db.Messages.SingleOrDefaultAsync(
            m => m.RecipientId == receiverId, ct);
}