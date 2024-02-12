using Contracts.Request;
using Contracts.Response;
using Logic.Abstractions;
using Models;
using Models.Enums;
using Newtonsoft.Json.Linq;
using PostgreSQL;
using PostgreSQL.Abstractions;
using System.Text;
using System.Threading.Tasks;

namespace Logic;

public sealed class SnapshotsHandler
    : ISnapshotsHandler
{
    public SnapshotsHandler(
        ICommonUsersUnitOfWork usersUnitOfWork)
    {
        _usersUnitOfWork = usersUnitOfWork
            ?? throw new ArgumentNullException(nameof(usersUnitOfWork));
    }

    private async Task UpdateEntitiesRepositorySnapshotsAsync(CancellationToken token)
    {
        _dbUsers = await _usersUnitOfWork
            .UsersRepository
            .GetAllUsersAsync(token);

        _dbTasks = await _usersUnitOfWork
            .TasksRepository
            .GetAllTasksAsync(token);

        _dbEvents = await _usersUnitOfWork
            .EventsRepository
            .GetAllEventsAsync(token);

        _dbIssues = await _usersUnitOfWork
            .IssuesRepository
            .GetAllIssuesAsync(token);

        _dbGroupsMaps =
            await _usersUnitOfWork
                .GroupingUsersMapRepository
                .GetAllMapsAsync(token);

        _dbEventsMaps = await _usersUnitOfWork
            .EventsUsersMapRepository
            .GetAllMapsAsync(token);
    }

    private async Task UpdateAllEntitiesRepositorySnapshotsAsync(CancellationToken token)
    {
        await UpdateEntitiesRepositorySnapshotsAsync(token);

        _dbGroupsMaps = await _usersUnitOfWork
            .GroupingUsersMapRepository
            .GetAllMapsAsync(token);
    }

    public async Task<SnapshotDescriptionResult> CreateSnapshotDescriptionAsync(
        int userId, 
        SnapshotInputDTO inputSnapshot,
        CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var beginMoment = inputSnapshot.BeginMoment;
        var endMoment = inputSnapshot.EndMoment;

        var snapshotType = inputSnapshot.SnapshotType;

        var creationTime = DateTimeOffset.UtcNow;

        await UpdateEntitiesRepositorySnapshotsAsync(token);

        return CreateSnapshotCertaintlyForUser(
            userId, snapshotType, creationTime, beginMoment, endMoment);
    }

    public async Task<GroupSnapshotDescriptionResult> CreateGroupKPISnapshotDescriptionAsync(
        int managerId,
        GroupSnapshotInputDTO inputSnapshot,
        CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var beginMoment = inputSnapshot.BeginMoment;
        var endMoment = inputSnapshot.EndMoment;

        var snapshotType = inputSnapshot.SnapshotType;

        var creationTime = DateTimeOffset.UtcNow;

        await UpdateAllEntitiesRepositorySnapshotsAsync(token);

        var participantsIds = _dbGroupsMaps!
            .Where(x => x.GroupId == inputSnapshot.GroupId)
            .Select(x => x.UserId)
            .OrderBy(x => x)
            .ToList();

        var separateParticipantsResults = participantsIds.Select(userId =>
        {
            return (CreateSnapshotCertaintlyForUser(
                userId, snapshotType, creationTime, beginMoment, endMoment), userId);
        });

        var averageKPI = separateParticipantsResults.Average(x => x.Item1.KPI);

        var stringBuilder = new StringBuilder();

        stringBuilder.Append($"Всего участников в группе {participantsIds.Count}\n\n");
        stringBuilder.Append("Список участников c их отчетами:\n");

        var i = 1;

        foreach (var participantMap in separateParticipantsResults)
        {
            var participantId = participantMap.userId;

            var currentUser = _dbUsers!.FirstOrDefault(x => x.Id == participantId)!;

            if (participantId == managerId)
            {
                stringBuilder.Append("(Менеджер группы) ");
            }

            stringBuilder.Append($"{i}. Участник: {currentUser.UserName} - с отчетом:\n");
            stringBuilder.Append($"{participantMap.Item1.Content}\n\n");
        }

        var groupSnapshot = new GroupSnapshotDescriptionResult
        {
            SnapshotType = snapshotType,
            BeginMoment = beginMoment,
            EndMoment = endMoment,
            CreateMoment = creationTime,
            Content = stringBuilder.ToString(),
            GroupId = inputSnapshot.GroupId,
            AverageKPI = averageKPI,
            ParticipantKPIs = separateParticipantsResults
                .Select(
                    map => new GroupParticipantKPIResponse 
                    { 
                        ParticipantId = map.userId,
                        ParticipantKPI = map.Item1.KPI
                    })
                .ToList(),
        };

        return groupSnapshot;
    }

    private SnapshotDescriptionResult CreateSnapshotCertaintlyForUser(
        int userId,
        SnapshotType snapshotType,
        DateTimeOffset creationTime,
        DateTimeOffset beginMoment,
        DateTimeOffset endMoment)
    {
        var user = _dbUsers!.FirstOrDefault(x => x.Id == userId);

        if (user != null)
        {
            if (snapshotType == SnapshotType.TasksSnapshot)
            {
                var userTasks = _dbTasks!
                    .Where(task => task.ImplementerId == userId)
                    .ToList();

                var stringBuilder = new StringBuilder();

                var notStartedTasks = userTasks
                    .Where(
                        x => x.TaskStatus == TaskCurrentStatus.ToDo)
                    .ToList();

                var inProgressTasks = userTasks
                    .Where(
                        x => x.TaskStatus == TaskCurrentStatus.InProgress)
                    .ToList();

                var doneTasks = userTasks
                    .Where(
                        x => x.TaskStatus == TaskCurrentStatus.Review
                        || x.TaskStatus == TaskCurrentStatus.Done)
                    .ToList();

                var tasksNotStartedPercent = userTasks.Count != 0
                    ? (notStartedTasks.Count / userTasks.Count) * 100
                    : 0;
                var tasksInProgressPercent = userTasks.Count != 0
                    ? (inProgressTasks.Count / userTasks.Count) * 100
                    : 0;
                var tasksDonePercent = userTasks.Count != 0
                    ? ((float)doneTasks.Count / userTasks.Count) * 100
                    : 0;

                stringBuilder.Append($"Отчет был выполнен в {creationTime.ToLocalTime()}\n");
                stringBuilder.Append($"Всего задач для имплементации пользователем - {userTasks.Count}\n");
                stringBuilder.Append($"Процент не начатых задач - {tasksNotStartedPercent}%\n");
                stringBuilder.Append($"Процент задач в процессе выполнения - {tasksInProgressPercent}%\n");
                stringBuilder.Append($"Процент задач, выполненных пользователем - {tasksDonePercent}%\n");

                var percent = (tasksDonePercent - tasksNotStartedPercent + tasksInProgressPercent / 2) / 100;

                var kpi = percent > 1 ? 1 : percent;

                var snapshotTasksResult = new SnapshotDescriptionResult
                {
                    SnapshotType = snapshotType,
                    BeginMoment = beginMoment,
                    EndMoment = endMoment,
                    CreateMoment = creationTime,
                    KPI = kpi,
                    Content = stringBuilder.ToString()
                };

                return snapshotTasksResult;
            }

            else if (snapshotType == SnapshotType.EventsSnapshot)
            {
                var userEventsMaps = _dbEventsMaps!
                    .Where(map => map.UserId == userId)
                    .ToList();

                var userEventsCount = 0;
                var totalEventsMinutesDuration = 0f;
                var acceptedEventsCount = 0;

                foreach (var map in userEventsMaps)
                {
                    var eventId = map.EventId;

                    var @event = _dbEvents!.FirstOrDefault(x => x.Id == eventId);

                    if (@event != null)
                    {
                        var eventStart = @event.ScheduledStart;
                        var eventEnd = eventStart.Add(@event.Duration);

                        var isFullEventEarlier = eventEnd <= beginMoment;

                        var isFullEventLater = eventStart >= endMoment;

                        if (isFullEventEarlier || isFullEventLater)
                        {
                            continue;
                        }

                        userEventsCount++;

                        var realEventDuration = (float)@event.Duration.TotalMinutes;

                        var partOfEventEarlierBeginning = beginMoment.Subtract(eventStart).TotalMinutes;

                        if (partOfEventEarlierBeginning > 0)
                        {
                            realEventDuration -= (float)partOfEventEarlierBeginning;
                        }

                        var partOfEventLaterEnd = eventEnd.Subtract(endMoment).TotalMinutes;

                        if (partOfEventLaterEnd > 0)
                        {
                            realEventDuration -= (float)partOfEventLaterEnd;
                        }

                        totalEventsMinutesDuration += realEventDuration;

                        if (map.DecisionType == DecisionType.Apply)
                        {
                            acceptedEventsCount++;
                        }
                    }
                }

                var stringBuilder = new StringBuilder();

                var totalHours = totalEventsMinutesDuration / 60;

                var eventsAcceptedPercent = userEventsCount == 0
                    ? ((float)acceptedEventsCount / userEventsCount) * 100
                    : 0;

                stringBuilder.Append($"Отчет был выполнен в {creationTime.ToLocalTime()}\n");
                stringBuilder.Append(
                    $"Всего запланированных мероприятий пользователя, за данный период - {userEventsCount}\n");
                stringBuilder.Append(
                    $"Общая продолжительность мероприятий за данный период - {totalHours.ToString("0.0")} ч.\n");
                stringBuilder.Append(
                    $"Процент посещенных мероприятий пользователем - {eventsAcceptedPercent}%\n");

                var percent = eventsAcceptedPercent / 100;

                var snapshotEventsResult = new SnapshotDescriptionResult
                {
                    SnapshotType = snapshotType,
                    BeginMoment = beginMoment,
                    EndMoment = endMoment,
                    CreateMoment = creationTime,
                    KPI = percent,
                    Content = stringBuilder.ToString()
                };

                return snapshotEventsResult;
            }

            else if (snapshotType == SnapshotType.IssuesSnapshot)
            {
                var userIssues = _dbIssues!.Where(x => x.Id == user.Id).ToList();

                var stringBuilder = new StringBuilder();

                var justReportedIssues = 
                    userIssues
                        .Where(x => x.Status == IssueStatus.Reported)
                        .ToList();

                var progressIssues =
                    userIssues
                        .Where(x => x.Status == IssueStatus.InProgress)
                        .ToList();

                var closedIssues =
                    userIssues
                        .Where(x => x.Status == IssueStatus.Closed)
                        .ToList();

                var issuesJustReportedPercent = userIssues.Count != 0
                    ? (justReportedIssues.Count / userIssues.Count) * 100
                    : 0;

                var issuesInProgressPercent = userIssues.Count != 0
                    ? (progressIssues.Count / userIssues.Count) * 100
                    : 0;

                var issuesClosedPercent = userIssues.Count != 0
                    ? (closedIssues.Count / userIssues.Count) * 100
                    : 0;

                stringBuilder.Append($"Отчет был выполнен в {creationTime.ToLocalTime()}\n");
                stringBuilder.Append($"Всего технических проблем было обнаружено пользователем - {userIssues.Count}\n");
                stringBuilder.Append($"Процент проблем, не взятых в просмотр - {issuesJustReportedPercent}%\n");
                stringBuilder.Append($"Процент проблем в процессе просмотра и выяснения - {issuesInProgressPercent}%\n");
                stringBuilder.Append($"Процент выясненных и закрытых проблем - {issuesClosedPercent}%\n");

                var percent = (issuesClosedPercent - issuesJustReportedPercent + issuesInProgressPercent / 2) / 100;

                var kpi = percent > 1 ? 1 : percent;

                var snapshotIssuesResult = new SnapshotDescriptionResult
                {
                    SnapshotType = snapshotType,
                    BeginMoment = beginMoment,
                    EndMoment = endMoment,
                    CreateMoment = creationTime,
                    KPI = kpi,
                    Content = stringBuilder.ToString(),
                };

                return snapshotIssuesResult;
            }

            else if (snapshotType == SnapshotType.ReportsSnapshot)
            {
                var snapshotReportsResult = new SnapshotDescriptionResult
                {
                    SnapshotType = snapshotType,
                    BeginMoment = beginMoment,
                    EndMoment = endMoment,
                    CreateMoment = creationTime,
                    KPI = 0,
                    Content = "Empty user reports content for current implementation stage"
                };

                return snapshotReportsResult;
            }

            else
            {
                throw new InvalidOperationException(
                    $"Such type as {snapshotType} is not supported" +
                    $" for receiving report content now");
            }
        }

        throw new ArgumentException($"User with id {userId} was not found");
    }

    private List<User>? _dbUsers;
    private List<UserTask>? _dbTasks;
    private List<Event>? _dbEvents;
    private List<Issue>? _dbIssues;
    private List<EventsUsersMap>? _dbEventsMaps;
    private List<GroupingUsersMap>? _dbGroupsMaps;

    private readonly ICommonUsersUnitOfWork _usersUnitOfWork;
}
