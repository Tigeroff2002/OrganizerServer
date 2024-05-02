using FirebaseAdmin.Auth;
using FirebaseAdmin.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Models;
using Models.StorageModels;
using PostgreSQL.Abstractions;

namespace PostgreSQL;

public sealed class MessageRepository
    : IMessageRepository
{
    public MessageRepository(
        ILogger<MessageRepository> logger,
        IServiceProvider provider)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var scope = _provider.GetRequiredService<IServiceScopeFactory>().CreateScope();
        _repositoryContext = scope.ServiceProvider.GetRequiredService<IRepositoryContext>();

        _logger.LogInformation("Message repository was created just now");
    }

    public async Task AddAsync(DirectMessage message, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        await _repositoryContext.Messages.AddAsync(message, token);
    }

    public async Task<DirectMessage?> GetMessageByIdAsync(int messageId, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        return await _repositoryContext.Messages
            .FirstOrDefaultAsync(x => x.Id == messageId);
    }

    public async Task<List<DirectMessage>> GetAllMessagesByChatIdAsync(int chatId, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var allMessages = await _repositoryContext.Messages.ToListAsync();

        return allMessages
            .Where(x => x.ChatId == chatId)
            .ToList();
    }

    public async Task<List<DirectMessage>> GetAllMessagesByUserIdAsync(int userId, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var allMessages = await _repositoryContext.Messages.ToListAsync();

        return allMessages
            .Where(x => x.UserId == userId)
            .ToList();
    }

    public async Task<List<DirectMessage>> GetAllMessagesByUserInOneChatAsync(
        int userId, int chatId, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var allMessages = await _repositoryContext.Messages.ToListAsync();

        return allMessages
            .Where(x => x.ChatId == chatId && x.UserId == userId)
            .ToList();
    }

    public async Task UpdateAsync(DirectMessage message, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(message);

        var existedMessage = await _repositoryContext.Messages.FirstOrDefaultAsync(
            x => x.Id == message.Id);

        if (existedMessage != null)
        {
            existedMessage.isEdited = true;
            existedMessage.Text = message.Text;
        }
    }

    public async Task DeleteAsync(int messageId, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var message = await _repositoryContext.Messages
            .FirstOrDefaultAsync(x => x.Id == messageId);

        if (message != null)
        {
            _repositoryContext.Messages.Entry(message).State = EntityState.Deleted;
        }
    }

    public void SaveChanges()
    {
        _repositoryContext.SaveChanges();

        _logger.LogInformation("The changes of messages were sent to DB");
    }

    private readonly ILogger<MessageRepository> _logger; 
    private readonly IServiceProvider _provider;
    private readonly IRepositoryContext _repositoryContext;
}
