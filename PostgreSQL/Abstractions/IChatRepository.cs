using FirebaseAdmin.Messaging;
using Models;
using Models.StorageModels;

namespace PostgreSQL.Abstractions;

public interface IChatRepository : IRepository
{
    Task AddAsync(DirectChat chat, CancellationToken token);

    Task<DirectChat?> GetChatByIdAsync(int chatId, CancellationToken token);

    Task<DirectChat?> GetChatByBothUserIdsAsync(int user1Id, int user2Id, CancellationToken token);

    Task<List<DirectChat>> GetAllChatsByUserIdAsync(int userId, CancellationToken token);

    Task UpdateNameAsync(int chatId, string newName, CancellationToken token);

    Task DeleteAsync(int chatId, CancellationToken token);
}
