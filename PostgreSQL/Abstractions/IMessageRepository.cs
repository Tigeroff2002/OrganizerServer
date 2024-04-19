using FirebaseAdmin.Messaging;
using Models;
using Models.StorageModels;

namespace PostgreSQL.Abstractions;

public interface IMessageRepository : IRepository
{
    Task AddAsync(DirectMessage message, CancellationToken token);

    Task<DirectMessage?> GetMessageByIdAsync(int messageId, CancellationToken token);

    Task<List<DirectMessage>> GetAllMessagesByChatIdAsync(int chatId, CancellationToken token);

    Task<List<DirectMessage>> GetAllMessagesByUserIdAsync(int userId, CancellationToken token);

    Task<List<DirectMessage>> GetAllMessagesByUserInOneChatAsync(int userId, int chatId, CancellationToken token);

    Task UpdateAsync(DirectMessage message, CancellationToken token);

    Task DeleteAsync(int messageId, CancellationToken token);
}
