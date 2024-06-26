﻿using Contracts.Request.RequestById;
using Contracts.Request;
using Contracts.Response;
using Logic.Abstractions;
using Models.Enums;
using Models.StorageModels;
using PostgreSQL.Abstractions;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Logic.ControllerHandlers;

public sealed class SnapshotsHandler
    : DataHandlerBase, ISnapshotsHandler
{
    public SnapshotsHandler(
        ICommonUsersUnitOfWork usersUnitOfWork,
        IRedisRepository redisRepository,
        ILogger<SnapshotsHandler> logger)
        : base(usersUnitOfWork, redisRepository, logger)
    {
    }

    public async Task<SnapshotDescriptionResult> CreatePersonalSnapshotDescriptionAsync(
        int userId,
        SnapshotInputDTO inputSnapshot,
        CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var beginMoment = inputSnapshot.BeginMoment;
        var endMoment = inputSnapshot.EndMoment;

        var snapshotType = inputSnapshot.SnapshotType;

        var auditType = SnapshotAuditType.Personal;

        var creationTime = DateTimeOffset.UtcNow;

        await UpdateEntitiesRepositorySnapshotsAsync(token);

        return CreateSnapshotCertaintlyForUser(
            userId, snapshotType, auditType, creationTime, beginMoment, endMoment);
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

        var auditType = SnapshotAuditType.Group;

        var creationTime = DateTimeOffset.UtcNow;

        await UpdateAllEntitiesRepositorySnapshotsAsync(token);

        var participantsIds = _dbGroupsMaps!
            .Where(x => x.GroupId == inputSnapshot.GroupId)
            .Select(x => x.UserId)
            .OrderBy(x => x)
            .ToList();

        var separateParticipantsResults = participantsIds.Select(userId =>
        {
            return (
                CreateSnapshotCertaintlyForUser(
                    userId, snapshotType, auditType, creationTime, beginMoment, endMoment),
                userId);
        });

        var averageKPI = separateParticipantsResults.Average(x => x.Item1.KPI);

        var stringBuilder = new StringBuilder();

        stringBuilder.Append($"Всего участников в группе: {participantsIds.Count}.\n\n");

        stringBuilder.Append($"Средний коэффициент KPI по группе равен: {averageKPI}.\n\n");

        stringBuilder.Append("Список участников c их отчетами:\n\n");

        var i = 1;

        foreach (var participantMap in separateParticipantsResults)
        {
            var participantId = participantMap.userId;

            var currentUser = _dbUsers!.FirstOrDefault(x => x.Id == participantId)!;

            if (participantId != managerId)
            {
                stringBuilder.Append($"{i}. Участник: {currentUser.UserName} - с отчетом:\n");
            }
            else
            {
                stringBuilder.Append($"{i}. Менеджер группы: {currentUser.UserName} - с отчетом:\n");
            }

            i++;

            var stringLines = participantMap.Item1.Content.Split('\n').Skip(1).ToList();

            var content = string.Join("\n", stringLines);

            stringBuilder.Append($"{content}\n");
            stringBuilder.Append($"Коэффициент KPI пользователя равен: {participantMap.Item1.KPI}\n");
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
                        ParticipantName = map.Item1.UserName,
                        ParticipantKPI = map.Item1.KPI
                    })
                .ToList(),
        };

        return groupSnapshot;
    }

    private SnapshotDescriptionResult CreateSnapshotCertaintlyForUser(
        int userId,
        SnapshotType snapshotType,
        SnapshotAuditType snapshotAuditType,
        DateTimeOffset creationTime,
        DateTimeOffset beginMoment,
        DateTimeOffset endMoment)
    {
        var user = _dbUsers!.FirstOrDefault(x => x.Id == userId);

        if (user != null)
        {
            var userName = user.UserName;

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

                var percent = (tasksDonePercent + tasksInProgressPercent / 2) / 100;

                if (float.IsNaN(percent))
                {
                    percent = 0f;
                }

                var snapshotTasksResult = new SnapshotDescriptionResult
                {
                    SnapshotType = snapshotType,
                    AuditType = snapshotAuditType,
                    BeginMoment = beginMoment,
                    EndMoment = endMoment,
                    CreateMoment = creationTime,
                    KPI = percent,
                    UserName = userName,
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

                var percent = (eventsAcceptedPercent + (100 - eventsAcceptedPercent) / 5) / 100;

                if (float.IsNaN(percent))
                {
                    percent = 0f;
                }

                var snapshotEventsResult = new SnapshotDescriptionResult
                {
                    SnapshotType = snapshotType,
                    AuditType = snapshotAuditType,
                    BeginMoment = beginMoment,
                    EndMoment = endMoment,
                    CreateMoment = creationTime,
                    KPI = percent,
                    UserName = userName,
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

                var percent = (issuesClosedPercent + issuesInProgressPercent / 2) / 100;

                if (float.IsNaN(percent))
                {
                    percent = 0;
                }

                var snapshotIssuesResult = new SnapshotDescriptionResult
                {
                    SnapshotType = snapshotType,
                    AuditType = snapshotAuditType,
                    BeginMoment = beginMoment,
                    EndMoment = endMoment,
                    CreateMoment = creationTime,
                    KPI = percent,
                    UserName = userName,
                    Content = stringBuilder.ToString(),
                };

                return snapshotIssuesResult;
            }

            else if (snapshotType == SnapshotType.ReportsSnapshot)
            {
                var snapshotReportsResult = new SnapshotDescriptionResult
                {
                    SnapshotType = snapshotType,
                    AuditType = snapshotAuditType,
                    BeginMoment = beginMoment,
                    EndMoment = endMoment,
                    CreateMoment = creationTime,
                    KPI = 0f,
                    UserName = userName,
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

    private async Task UpdateEntitiesRepositorySnapshotsAsync(
    CancellationToken token)
    {
        _dbUsers = await CommonUnitOfWork
            .UsersRepository
            .GetAllUsersAsync(token);

        _dbTasks = await CommonUnitOfWork
            .TasksRepository
            .GetAllTasksAsync(token);

        _dbEvents = await CommonUnitOfWork
            .EventsRepository
            .GetAllEventsAsync(token);

        _dbIssues = await CommonUnitOfWork
            .IssuesRepository
            .GetAllIssuesAsync(token);

        _dbGroupsMaps =
            await CommonUnitOfWork
                .GroupingUsersMapRepository
                .GetAllMapsAsync(token);

        _dbEventsMaps = await CommonUnitOfWork
            .EventsUsersMapRepository
            .GetAllMapsAsync(token);
    }

    private async Task UpdateAllEntitiesRepositorySnapshotsAsync(CancellationToken token)
    {
        await UpdateEntitiesRepositorySnapshotsAsync(token);

        _dbGroupsMaps = await CommonUnitOfWork
            .GroupingUsersMapRepository
            .GetAllMapsAsync(token);
    }

    private List<User>? _dbUsers;
    private List<UserTask>? _dbTasks;
    private List<Event>? _dbEvents;
    private List<Issue>? _dbIssues;
    private List<EventsUsersMap>? _dbEventsMaps;
    private List<GroupingUsersMap>? _dbGroupsMaps;
}
