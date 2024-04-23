using Contracts.RedisContracts.AlertEvents;
using Contracts.RedisContracts.GroupEvents;
using Contracts.RedisContracts.IssueEvents;
using Contracts.RedisContracts.MeetEvents;
using Contracts.RedisContracts.TaskEvents;
using Contracts.RedisContracts.UserEvents;
using Contracts.RedisContracts;
using Logic.Transport.Abstractions;
using Models.RedisEventModels.AlertEvents;
using Models.RedisEventModels.GroupEvents;
using Models.RedisEventModels.IssueEvents;
using Models.RedisEventModels.MeetEvents;
using Models.RedisEventModels.TaskEvents;
using Models.RedisEventModels.UserEvents;
using Models.RedisEventModels;
using Newtonsoft.Json;
using System.Text;

namespace Logic.Transport.Serialization;

public sealed class RedisEventSerializer
    : ISerializer<BaseEvent>
{
    public string Serialize(BaseEvent entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        BaseEventDTO dto = entity.GetType() switch
        {
            _ when entity is AlertCreatedEvent alertCreated
                => new AlertCreatedEventDTO
                {
                    Id = alertCreated.Id,
                    IsCommited = alertCreated.IsCommited,
                    UserId = alertCreated.Id,
                    EventType = RawEventType.AlertCreated,
                    AlertId = alertCreated.AlertId,
                    CreatedMoment = alertCreated.CreateMoment
                },

            _ when entity is GroupCreatedEvent groupCreated
                => new GroupCreatedEventDTO
                {
                    Id = groupCreated.Id,
                    IsCommited = groupCreated.IsCommited,
                    UserId = groupCreated.Id,
                    EventType = RawEventType.GroupCreated,
                    GroupId = groupCreated.GroupId,
                    CreatedMoment = groupCreated.CreatedMoment
                },
            _ when entity is GroupParamsChangedEvent groupUpdated
                => new GroupParamsChangedEventDTO
                {
                    Id = groupUpdated.Id,
                    IsCommited = groupUpdated.IsCommited,
                    UserId = groupUpdated.Id,
                    EventType = RawEventType.GroupParamsChanged,
                    GroupId = groupUpdated.GroupId,
                    UpdateMoment = groupUpdated.UpdateMoment,
                    Json = groupUpdated.Json
                },
            _ when entity is GroupParticipantDeletedEvent groupParticipantDeleted
                => new GroupParticipantDeletedEventDTO
                {
                    Id = groupParticipantDeleted.Id,
                    IsCommited = groupParticipantDeleted.IsCommited,
                    UserId = groupParticipantDeleted.Id,
                    EventType = RawEventType.GroupParticipantDeleted,
                    GroupId = groupParticipantDeleted.GroupId
                },
            _ when entity is GroupParticipantInvitedEvent groupParticipantInvited
                => new GroupParticipantInvitedEventDTO
                {
                    Id = groupParticipantInvited.Id,
                    IsCommited = groupParticipantInvited.IsCommited,
                    UserId = groupParticipantInvited.Id,
                    EventType = RawEventType.GroupParticipantInvited,
                    GroupId = groupParticipantInvited.GroupId
                },

            _ when entity is IssueCreatedEvent issueCreated
                => new IssueCreatedEventDTO
                {
                    Id = issueCreated.Id,
                    IsCommited = issueCreated.IsCommited,
                    UserId = issueCreated.Id,
                    EventType = RawEventType.IssueCreated,
                    IssueId = issueCreated.IssueId,
                    CreatedMoment = issueCreated.CreatedMoment
                },
            _ when entity is IssueParamsChangedEvent issueUpdated
                => new IssueParamsChangedEventDTO
                {
                    Id = issueUpdated.Id,
                    IsCommited = issueUpdated.IsCommited,
                    UserId = issueUpdated.Id,
                    EventType = RawEventType.IssueParamsChanged,
                    IssueId = issueUpdated.IssueId,
                    UpdateMoment = issueUpdated.UpdateMoment,
                    Json = issueUpdated.Json
                },
            _ when entity is IssueTerminalStatusReceivedEvent issueTerminaled
                => new IssueTerminalStatusReceivedEventDTO
                {
                    Id = issueTerminaled.Id,
                    IsCommited = issueTerminaled.IsCommited,
                    UserId = issueTerminaled.Id,
                    EventType = RawEventType.IssueTerminalStatusReceived,
                    IssueId = issueTerminaled.IssueId,
                    TerminalMoment = issueTerminaled.TerminalMoment,
                    TerminalStatus = issueTerminaled.TerminalStatus
                },

            _ when entity is MeetCreatedEvent meetCreated
                => new MeetCreatedEventDTO
                {
                    Id = meetCreated.Id,
                    IsCommited = meetCreated.IsCommited,
                    UserId = meetCreated.Id,
                    EventType = RawEventType.MeetCreated,
                    MeetId = meetCreated.MeetId,
                    CreatedMoment = meetCreated.CreatedMoment,
                    ScheduledStart = meetCreated.ScheduledStart,
                    Duration = meetCreated.Duration
                },
            _ when entity is MeetGuestDeletedEvent meetGuestDeleted
                => new MeetGuestDeletedEventDTO
                {
                    Id = meetGuestDeleted.Id,
                    IsCommited = meetGuestDeleted.IsCommited,
                    UserId = meetGuestDeleted.Id,
                    EventType = RawEventType.MeetGuestDeleted,
                    MeetId = meetGuestDeleted.MeetId
                },
            _ when entity is MeetGuestInvitedEvent meetGuestInvited
                => new MeetGuestInvitedEventDTO
                {
                    Id = meetGuestInvited.Id,
                    IsCommited = meetGuestInvited.IsCommited,
                    UserId = meetGuestInvited.Id,
                    EventType = RawEventType.MeetGuestInvited,
                    MeetId = meetGuestInvited.MeetId
                },
            _ when entity is MeetParamsChangedEvent meetUpdated
                => new MeetParamsChangedEventDTO
                {
                    Id = meetUpdated.Id,
                    IsCommited = meetUpdated.IsCommited,
                    UserId = meetUpdated.Id,
                    EventType = RawEventType.MeetParamsChanged,
                    MeetId = meetUpdated.MeetId,
                    UpdateMoment = meetUpdated.UpdateMoment,
                    Json = meetUpdated.Json
                },
            _ when entity is MeetSoonBeginEvent meetSoonBegin
                => new MeetSoonBeginEventDTO
                {
                    Id = meetSoonBegin.Id,
                    IsCommited = meetSoonBegin.IsCommited,
                    UserId = meetSoonBegin.Id,
                    EventType = RawEventType.MeetSoonBegin,
                    MeetId = meetSoonBegin.MeetId,
                    RemainingTime = meetSoonBegin.RemainingTime,
                    ScheduledStart = meetSoonBegin.ScheduledStart
                },
            _ when entity is MeetTerminalStatusReceivedEvent meetTerminaled
                => new MeetTerminalStatusReceivedEventDTO
                {
                    Id = meetTerminaled.Id,
                    IsCommited = meetTerminaled.IsCommited,
                    UserId = meetTerminaled.Id,
                    EventType = RawEventType.MeetTerminalStatusReceived,
                    MeetId = meetTerminaled.MeetId,
                    TerminalMoment = meetTerminaled.TerminalMoment,
                    TerminalStatus = meetTerminaled.TerminalStatus
                },

            _ when entity is TaskAssignedEvent taskAssigned
                => new TaskAssignedEventDTO
                {
                    Id = taskAssigned.Id,
                    IsCommited = taskAssigned.IsCommited,
                    UserId = taskAssigned.Id,
                    EventType = RawEventType.TaskAssignedEvent,
                    TaskId = taskAssigned.TaskId
                },

            _ when entity is TaskCreatedEvent taskCreated
                => new TaskCreatedEventDTO
                {
                    Id = taskCreated.Id,
                    IsCommited = taskCreated.IsCommited,
                    UserId = taskCreated.Id,
                    EventType = RawEventType.TaskCreatedEvent,
                    TaskId = taskCreated.TaskId,
                    CreatedMoment = taskCreated.CreatedMoment
                },
            _ when entity is TaskParamsChangedEvent taskUpdated
                => new TaskParamsChangedEventDTO
                {
                    Id = taskUpdated.Id,
                    IsCommited = taskUpdated.IsCommited,
                    UserId = taskUpdated.Id,
                    EventType = RawEventType.TaskParamsChanged,
                    TaskId = taskUpdated.TaskId,
                    UpdateMoment = taskUpdated.UpdateMoment,
                    Json = taskUpdated.Json
                },
            _ when entity is TaskTerminalStatusReceivedEvent taskTerminaled
                => new TaskTerminalStatusReceivedEventDTO
                {
                    Id = taskTerminaled.Id,
                    IsCommited = taskTerminaled.IsCommited,
                    UserId = taskTerminaled.Id,
                    EventType = RawEventType.TaskTerminalStatusReceived,
                    TaskId = taskTerminaled.TaskId,
                    TerminalMoment = taskTerminaled.TerminalMoment,
                    TerminalStatus = taskTerminaled.TerminalStatus
                },
            _ when entity is TaskUnassignedEvent taskUnassigned
                => new TaskUnassignedEventDTO
                {
                    Id = taskUnassigned.Id,
                    IsCommited = taskUnassigned.IsCommited,
                    UserId = taskUnassigned.Id,
                    EventType = RawEventType.TaskUnassignedEvent,
                    TaskId = taskUnassigned.TaskId
                },

            _ when entity is UserInfoUpdateEvent userUpdated
                => new UserInfoUpdateEventDTO
                {
                    Id = userUpdated.Id,
                    IsCommited = userUpdated.IsCommited,
                    UserId = userUpdated.Id,
                    EventType = RawEventType.UserInfoUpdate,
                    UpdateMoment = userUpdated.UpdateMoment,
                    Json = userUpdated.Json
                },

            _ when entity is UserLoginEvent userLogin
                => new UserLoginEventDTO
                {
                    Id = userLogin.Id,
                    IsCommited = userLogin.IsCommited,
                    UserId = userLogin.Id,
                    EventType = RawEventType.UserLogin,
                    FirebaseToken = userLogin.FirebaseToken,
                    TokenSetMoment = userLogin.TokenSetMoment
                },
            _ when entity is UserLogoutEvent userLogout
                => new UserLogoutEventDTO
                {
                    Id = userLogout.Id,
                    IsCommited = userLogout.IsCommited,
                    UserId = userLogout.Id,
                    EventType = RawEventType.UserLogout,
                    FirebaseToken = userLogout.FirebaseToken
                },
            _ when entity is UserRegistrationEvent userRegister
                => new UserRegistrationEventDTO
                {
                    Id = userRegister.Id,
                    IsCommited = userRegister.IsCommited,
                    UserId = userRegister.Id,
                    EventType = RawEventType.UserRegistration,
                    FirebaseToken = userRegister.FirebaseToken,
                    AccountCreationMoment = userRegister.AccountCreationMoment
                },
            _ when entity is UserRoleChangedEvent userRoleChanged
                => new UserRoleChangedEventDTO
                {
                    Id = userRoleChanged.Id,
                    IsCommited = userRoleChanged.IsCommited,
                    UserId = userRoleChanged.Id,
                    EventType = RawEventType.UserRoleChanged,
                    UpdateMoment = userRoleChanged.UpdateMoment,
                    NewRole = userRoleChanged.NewRole
                },

            _ => throw new InvalidOperationException(
                $"Not supported type: {entity.GetType().Name} was found.")
        };

        var buffer = new StringBuilder();

        using var writer = new JsonTextWriter(new StringWriter(buffer));

        _serializer.Serialize(writer, dto);

        return buffer.ToString();
    }

    private static readonly JsonSerializer _serializer = JsonSerializer.CreateDefault();
}
