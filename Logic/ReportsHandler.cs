using Contracts.Request;
using Contracts.Response;
using Logic.Abstractions;
using Models;
using Models.Enums;
using PostgreSQL.Abstractions;
using System.Text;
using System.Threading.Tasks;

namespace Logic;

public sealed class ReportsHandler
    : IReportsHandler
{
    public ReportsHandler(
        IUsersRepository usersRepository,
        IEventsRepository eventsRepository,
        IEventsUsersMapRepository eventsUsersMapRepository,
        ITasksRepository tasksRepository,
        IGroupsRepository groupsRepository,
        IGroupingUsersMapRepository groupingUsersMapRepository)
    {
        _usersRepository = usersRepository 
            ?? throw new ArgumentNullException(nameof(usersRepository));

        _eventsRepository = eventsRepository
            ?? throw new ArgumentNullException(nameof(eventsRepository));

        _eventsUsersMapRepository = eventsUsersMapRepository
            ?? throw new ArgumentNullException(nameof(eventsUsersMapRepository));

        _tasksRepository = tasksRepository
            ?? throw new ArgumentNullException(nameof(tasksRepository));

        _groupsRepository = groupsRepository
            ?? throw new ArgumentNullException(nameof(groupsRepository));

        _groupingUsersMapRepository = groupingUsersMapRepository
            ?? throw new ArgumentNullException(nameof(groupingUsersMapRepository));
    }

    public async Task<ReportDescriptionResult> CreateReportDescriptionAsync(
        int userId, 
        ReportInputDTO inputReport,
        CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var user = await _usersRepository.GetUserByIdAsync(userId, token);

        var beginMoment = inputReport.BeginMoment;
        var endMoment = inputReport.EndMoment;

        var reportType = inputReport.ReportType;

        var creationTime = DateTimeOffset.UtcNow;

        if (user != null)
        {
            if (reportType == ReportType.TasksReport)
            {
                var dbTasks = await _tasksRepository.GetAllTasksAsync(token);

                var userTasks = dbTasks
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

                var reportTasksResult = new ReportDescriptionResult
                {
                    ReportType = reportType,
                    BeginMoment = beginMoment,
                    EndMoment = endMoment,
                    Content = stringBuilder.ToString()
                };

                return reportTasksResult;
            }

            else if (reportType == ReportType.EventsReport)
            {
                var groupsUsersMaps =
                    await _groupingUsersMapRepository.GetAllMapsAsync(token);

                var dbMaps = await _eventsUsersMapRepository.GetAllMapsAsync(token);

                var userEventsMaps = dbMaps
                    .Where(map => map.UserId == userId)
                    .ToList();

                var userEventsCount = 0;
                var totalEventsMinutesDuration = 0f;
                var acceptedEventsCount = 0;

                foreach (var map in userEventsMaps)
                {
                    var eventId = map.EventId;

                    var @event = await _eventsRepository
                        .GetEventByIdAsync(eventId, token);

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

                        var realEventDuration = (float) @event.Duration.TotalMinutes;

                        var partOfEventEarlierBeginning = beginMoment.Subtract(eventStart).TotalMinutes;

                        if (partOfEventEarlierBeginning > 0)
                        {
                            realEventDuration -= (float) partOfEventEarlierBeginning;
                        }

                        var partOfEventLaterEnd = eventEnd.Subtract(endMoment).TotalMinutes;

                        if (partOfEventLaterEnd > 0)
                        {
                            realEventDuration -= (float) partOfEventLaterEnd;
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

                var reportEventsResult = new ReportDescriptionResult
                {
                    ReportType = reportType,
                    BeginMoment = beginMoment,
                    EndMoment = endMoment,
                    Content = stringBuilder.ToString()
                };

                return reportEventsResult;
            }
            else
            {
                throw new InvalidOperationException(
                    $"Such type as {reportType} is not supported" +
                    $" for receiving report content now");
            }
        }

        throw new ArgumentException($"User with id {userId} was not found");
    }

    private readonly IUsersRepository _usersRepository;
    private readonly IEventsRepository _eventsRepository;
    private readonly IEventsUsersMapRepository _eventsUsersMapRepository;
    private readonly ITasksRepository _tasksRepository;
    private readonly IGroupsRepository _groupsRepository;
    private readonly IGroupingUsersMapRepository _groupingUsersMapRepository;
}
