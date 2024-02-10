using Microsoft.Extensions.Logging;
using PostgreSQL.Abstractions;

namespace PostgreSQL;

public sealed class CommonUsersUnitOfWork
    : ICommonUsersUnitOfWork
{
    public IUsersRepository UsersRepository { get; }

    public IUserDevicesRepository UserDevicesRepository { get; }

    public IGroupsRepository GroupsRepository { get; }

    public ITasksRepository TasksRepository { get; }

    public IEventsRepository EventsRepository { get; }

    public IEventsUsersMapRepository EventsUsersMapRepository { get; }

    public IGroupingUsersMapRepository GroupingUsersMapRepository { get; }

    public ISnapshotsRepository SnapshotsRepository { get; }

    public IIssuesRepository IssuesRepository { get; }

    public CommonUsersUnitOfWork(
        IUsersRepository usersRepository,
        IUserDevicesRepository userDevicesRepository,
        IGroupsRepository groupsRepository,
        IEventsRepository eventsRepository,
        ITasksRepository tasksRepository,
        IEventsUsersMapRepository eventsUsersMapRepository,
        IGroupingUsersMapRepository groupingUsersMapRepository,
        ISnapshotsRepository snapshotsRepository,
        IIssuesRepository issuesRepository,
        ILogger<CommonUsersUnitOfWork> logger)
    {
        UsersRepository = usersRepository
            ?? throw new ArgumentNullException(nameof(usersRepository));

        UserDevicesRepository = userDevicesRepository
            ?? throw new ArgumentNullException(nameof(userDevicesRepository));

        GroupsRepository = groupsRepository
            ?? throw new ArgumentNullException(nameof(groupsRepository));

        EventsRepository = eventsRepository
            ?? throw new ArgumentNullException(nameof(eventsRepository));

        TasksRepository = tasksRepository
            ?? throw new ArgumentNullException(nameof(tasksRepository));

        EventsUsersMapRepository = eventsUsersMapRepository
            ?? throw new ArgumentNullException(nameof(eventsUsersMapRepository));

        GroupingUsersMapRepository = groupingUsersMapRepository
            ?? throw new ArgumentNullException(nameof(groupingUsersMapRepository));

        SnapshotsRepository = snapshotsRepository
            ?? throw new ArgumentNullException(nameof(snapshotsRepository));

        IssuesRepository = issuesRepository
            ?? throw new ArgumentNullException(nameof(issuesRepository));

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void SaveChanges()
    {
        UsersRepository.SaveChanges();
        UserDevicesRepository.SaveChanges();
        GroupsRepository.SaveChanges();
        EventsRepository.SaveChanges();
        TasksRepository.SaveChanges();
        EventsUsersMapRepository.SaveChanges();
        GroupingUsersMapRepository.SaveChanges();
        SnapshotsRepository.SaveChanges();
        IssuesRepository.SaveChanges();

        _logger.LogInformation("The changes of users unit of work were accepted");
    }

    private readonly ILogger _logger;
}
