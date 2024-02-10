using Microsoft.Extensions.Logging;
using PostgreSQL.Abstractions;

namespace PostgreSQL;

public sealed class UsersMessagingUnitOfWork
    : IUsersMessagingUnitOfWork
{
    public IUsersRepository UsersRepository { get; }

    public IChatRepository ChatRepository { get; }

    public IMessageRepository MessageRepository { get; }

    public UsersMessagingUnitOfWork(
        IUsersRepository usersRepository, 
        IChatRepository chatRepository,
        IMessageRepository messageRepository,
        ILogger<UsersMessagingUnitOfWork> logger)
    {
        UsersRepository = usersRepository 
            ?? throw new ArgumentNullException(nameof(usersRepository));
        ChatRepository = chatRepository 
            ?? throw new ArgumentNullException(nameof(chatRepository));
        MessageRepository = messageRepository 
            ?? throw new ArgumentNullException(nameof(messageRepository));

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void SaveChanges()
    {
        UsersRepository.SaveChanges();
        ChatRepository.SaveChanges();
        MessageRepository.SaveChanges();

        _logger.LogInformation("The changes of users messaing unit of work were accepted");
    }

    private readonly ILogger _logger;
}
