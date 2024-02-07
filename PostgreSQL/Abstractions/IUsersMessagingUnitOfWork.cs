namespace PostgreSQL.Abstractions;

public interface IUsersMessagingUnitOfWork
{
    public IUsersRepository UsersRepository { get; }

    public IChatRepository ChatRepository { get; }

    public IMessageRepository MessageRepository { get; }

    public void SaveChanges();
}
