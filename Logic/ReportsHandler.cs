using Contracts.Response;
using Logic.Abstractions;
using Models;
using Models.Enums;
using PostgreSQL.Abstractions;
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
        ReportType reportType,
        CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var user = await _usersRepository.GetUserByIdAsync(userId, token);

        var creationTime = DateTimeOffset.UtcNow;

        if (user != null)
        {
            if (reportType == ReportType.TasksReport)
            {
                var dbTasks = await _tasksRepository.GetAllTasksAsync(token);

                var userTasks = dbTasks
                    .Where(task => task.ImplementerId == userId)
                    .ToList();

                var tasksInfo = new List<TaskInfoResponse>();

                foreach (var task in userTasks)
                { 
                    var reporter = await _usersRepository
                        .GetUserByIdAsync(task.ReporterId, token);

                    ShortUserInfo reporterInfo =
                        reporter != null
                            ? new ShortUserInfo
                            {
                                UserEmail = reporter.Email,
                                UserName = reporter.UserName,
                                UserPhone = reporter.PhoneNumber
                            }
                            : null!;

                    var implementerInfo = new ShortUserInfo
                    {
                        UserEmail = user.Email,
                        UserName = user.UserName,
                        UserPhone = user.PhoneNumber
                    };

                    var info = new TaskInfoResponse
                    {
                        TaskCaption = task.Caption,
                        TaskDescription = task.Description,
                        TaskStatus = task.TaskStatus,
                        TaskType = task.TaskType,
                        Reporter = reporterInfo,
                        Implementer = implementerInfo
                    };

                    tasksInfo.Add(info);
                }

                var reportTasksResult = new ReportTasksDescriptionResult
                {
                    CreationTime = creationTime,
                    TasksInformation = tasksInfo
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

                var eventsInfo = new List<EventInfoResponse>();

                foreach (var map in userEventsMaps)
                {
                    var eventId = map.EventId;

                    var @event = await _eventsRepository
                        .GetEventByIdAsync(eventId, token);

                    if (@event != null)
                    {
                        var manager = await _usersRepository
                            .GetUserByIdAsync(@event.ManagerId, token);

                        ShortUserInfo managerInfo =
                            manager != null
                                ? new ShortUserInfo
                                {
                                    UserEmail = manager.Email,
                                    UserName = manager.UserName,
                                    UserPhone = manager.PhoneNumber
                                }
                                : null!;

                        var group = await _groupsRepository
                            .GetGroupByIdAsync(@event.RelatedGroupId, token);

                        GroupInfoResponse groupInfo = null!;

                        if (group != null)
                        {
                            var participants = new List<ShortUserInfo>();

                            var groupId = group.Id;

                            var usersMaps = groupsUsersMaps
                                .Where(map => map.GroupId == groupId).ToList();

                            foreach (var participantMap in usersMaps)
                            {
                                var participantId = participantMap.UserId;

                                var participant = await _usersRepository
                                    .GetUserByIdAsync(participantId, token);

                                if (participant != null)
                                {
                                    var participantInfo = new ShortUserInfo
                                    {
                                        UserEmail = participant.Email,
                                        UserName = participant.UserName,
                                        UserPhone = participant.PhoneNumber
                                    };

                                    participants.Add(participantInfo);
                                }
                            }

                            groupInfo = new GroupInfoResponse
                            {
                                GroupName = group.GroupName,
                                Type = group.Type,
                                Participants = participants
                            };
                        }

                        var eventGuestsMaps = dbMaps
                            .Where(map => map.EventId == @event.Id)
                            .ToList();

                        var guestsInfoWithDecisions = new List<UserInfoWithDecision>();

                        foreach (var guestMap in eventGuestsMaps)
                        {
                            var guestId = guestMap.UserId;

                            var guest = await _usersRepository
                                .GetUserByIdAsync(guestId, token);

                            if (guest != null)
                            {
                                var guestInfo = new UserInfoWithDecision
                                {
                                    UserEmail = guest.Email,
                                    UserName = guest.UserName,
                                    UserPhone = guest.PhoneNumber,
                                    DecisionType = guestMap.DecisionType
                                };

                                guestsInfoWithDecisions.Add(guestInfo);
                            }
                        }

                        var info = new EventInfoResponse
                        {
                            Caption = @event.Caption,
                            Description = @event.Description,
                            EventType = @event.EventType,
                            EventStatus = @event.Status,
                            ScheduledStart = @event.ScheduledStart,
                            Duration = @event.Duration,
                            Manager = managerInfo,
                            Group = groupInfo,
                            Guests = guestsInfoWithDecisions
                        };

                        eventsInfo.Add(info);
                    }
                }

                var reportEventsResult = new ReportEventsDescriptionResult
                {
                    CreationTime = creationTime,
                    EventsInformation = eventsInfo
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
