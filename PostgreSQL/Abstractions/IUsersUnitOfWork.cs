namespace PostgreSQL.Abstractions;

public interface IUsersUnitOfWork
{
    public IUsersRepository UsersRepository { get; }

    public IGroupsRepository GroupsRepository { get; }

    public ITasksRepository TasksRepository { get; }

    public IEventsRepository EventsRepository { get; }

    public IEventsUsersMapRepository EventsUsersMapRepository { get; }

    public IGroupingUsersMapRepository GroupingUsersMapRepository { get; }

    public ISnapshotsRepository SnapshotsRepository { get; }

    public IIssuesRepository IssuesRepository { get; }

    public void SaveChanges();
}
