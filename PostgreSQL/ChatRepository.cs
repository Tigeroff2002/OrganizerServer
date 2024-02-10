using FirebaseAdmin.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Models;
using PostgreSQL.Abstractions;

namespace PostgreSQL;

public sealed class ChatRepository
    : IChatRepository
{
    public ChatRepository(
        ILogger<ChatRepository> logger,
        IServiceProvider provider)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var scope = _provider.GetRequiredService<IServiceScopeFactory>().CreateScope();
        _repositoryContext = scope.ServiceProvider.GetRequiredService<IRepositoryContext>();

        _logger.LogInformation("Chat repository was created just now");
    }

    public async Task AddAsync(DirectChat chat, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        await _repositoryContext.DirectChats.AddAsync(chat, token);
    }

    public async Task<DirectChat?> GetChatByIdAsync(int chatId, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        return await _repositoryContext.DirectChats
            .FirstOrDefaultAsync(x => x.Id == chatId);
    }

    public async Task<DirectChat?> GetChatByBothUserIdsAsync(int user1Id, int user2Id, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var allChats = await _repositoryContext.DirectChats.ToListAsync();

        return allChats
            .FirstOrDefault(
                x => x.User1Id == user1Id && x.User2Id == user2Id
                    || x.User1Id == user2Id && x.User2Id == user1Id);
    }

    public async Task<List<DirectChat>> GetAllChatsByUserIdAsync(int userId, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var allChats = await _repositoryContext.DirectChats.ToListAsync();

        return allChats
            .Where(x => x.User1Id == userId || x.User2Id == userId)
            .ToList();
    }

    public async Task UpdateNameAsync(int chatId, string newName, CancellationToken token)
    {
        var existedChat = await _repositoryContext.DirectChats.FirstOrDefaultAsync(
            x => x.Id == chatId);

        if (existedChat != null)
        {
            existedChat.Caption = newName;
        }
    }

    public async Task DeleteAsync(int chatId, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var chat = await _repositoryContext.DirectChats
            .FirstOrDefaultAsync(x => x.Id == chatId);

        if (chat != null)
        {
            _repositoryContext.DirectChats.Entry(chat).State = EntityState.Deleted;
        }
    }

    public void SaveChanges()
    {
        _repositoryContext.SaveChanges();

        _logger.LogInformation("The changes of direct chats were sent to DB");
    }


    private readonly ILogger<ChatRepository> _logger;
    private readonly IServiceProvider _provider;
    private readonly IRepositoryContext _repositoryContext;
}
