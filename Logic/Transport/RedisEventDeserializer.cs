using Contracts.RedisContracts;
using Contracts.RedisContracts.AlertEvents;
using Contracts.RedisContracts.GroupEvents;
using Contracts.RedisContracts.IssueEvents;
using Contracts.RedisContracts.MeetEvents;
using Contracts.RedisContracts.SnapshotEvents;
using Contracts.RedisContracts.TaskEvents;
using Contracts.RedisContracts.UserEvents;

using Logic.Transport.Abstractions;

using Models.RedisEventModels;
using Models.RedisEventModels.AlertEvents;
using Models.RedisEventModels.GroupEvents;
using Models.RedisEventModels.IssueEvents;
using Models.RedisEventModels.MeetEvents;
using Models.RedisEventModels.SnapshotEvents;
using Models.RedisEventModels.TaskEvents;
using Models.RedisEventModels.UserEvents;

using Newtonsoft.Json;
using System.Diagnostics;

namespace Logic.Transport;

public sealed class RedisEventDeserializer
    : IDeserializer<BaseEvent>
{
    public BaseEvent Deserialize(string source)
    {
        ArgumentException.ThrowIfNullOrEmpty(source, nameof(source));

        using var reader = new JsonTextReader(new StringReader(source));

        var rawDTO = _deserializer.Deserialize<BaseEventDTO>(reader);

        Debug.Assert(rawDTO is not null);

        return rawDTO.EventType switch
        {
            RawEventType.AlertCreated => 
                CreateAlertCreatedFromSource(reader),

            RawEventType.GroupCreated => 
                CreateGroupCreatedFromSource(reader),

            RawEventType.GroupParamsChanged => 
                CreateGroupUpdatedFromSource(reader),

            RawEventType.GroupParticipantDeleted => 
                CreateParticipantDeletedCreatedFromSource(reader),

            RawEventType.GroupParticipantInvited => 
                CreateParticipantInvitedFromSource(reader),

            RawEventType.IssueCreated => 
                CreateIssueCreatedFromSource(reader),

            RawEventType.IssueParamsChanged => 
                CreateIssueUpdatedFromSource(reader),

            RawEventType.IssueTerminalStatusReceived => 
                CreateIssueTerminatedFromSource(reader),

            RawEventType.MeetCreated => 
                CreateMeetCreatedFromSource(reader),

            RawEventType.MeetGuestDeleted => 
                CreateMeetGuestDeletedFromSource(reader),

            RawEventType.MeetGuestInvited => 
                CreateMeetGuestInvitedFromSource(reader),

            RawEventType.MeetParamsChanged => 
                CreateMeetUpdatedFromSource(reader),

            RawEventType.MeetSoonBegin => 
                CreateMeetSoonBeginFromSource(reader),

            RawEventType.MeetTerminalStatusReceived => 
                CreateMeetTerminatedFromSource(reader),

            RawEventType.SnapshotCreated => 
                CreateSnapshotCreatedFromSource(reader),

            RawEventType.TaskAssignedEvent => 
                CreateTaskAssignedFromSource(reader),

            RawEventType.TaskCreatedEvent => 
                CreateTaskCreatedFromSource(reader),

            RawEventType.TaskParamsChanged => 
                CreateTaskUpdatedFromSource(reader),

            RawEventType.TaskTerminalStatusReceived => 
                CreateTaskTerminatedFromSource(reader),

            RawEventType.TaskUnassignedEvent =>
                CreateTaskUnassignedFromSource(reader),

            RawEventType.UserInfoUpdate => CreateUserUpdatedFromSource(reader),

            RawEventType.UserLogin => CreateUserLoginFromSource(reader),

            RawEventType.UserLogout => CreateUserLogoutFromSource(reader),

            RawEventType.UserRegistration => CreateUserRegisterFromSource(reader),

            RawEventType.UserRoleChanged => CreateUserRoleChangedFromSource(reader),

            _ => throw new NotSupportedException(
                $"Not supported event type {rawDTO.EventType} was found on cache.")
        };
    }

    private AlertCreatedEvent CreateAlertCreatedFromSource(
        JsonTextReader reader)
    {
        var dto = _deserializer.Deserialize<AlertCreatedEventDTO>(reader);

        Debug.Assert(dto is not null);

        return new(
            dto.Id, 
            dto.IsCommited,
            dto.UserId,
            dto.AlertId,
            dto.CreatedMoment);
    }

    private GroupCreatedEvent CreateGroupCreatedFromSource(
        JsonTextReader reader)
    {
        var dto = _deserializer.Deserialize<GroupCreatedEventDTO>(reader);

        Debug.Assert(dto is not null);

        return new(
            dto.Id,
            dto.IsCommited, 
            dto.UserId,
            dto.GroupId, 
            dto.CreatedMoment);
    }

    private GroupParamsChangedEvent CreateGroupUpdatedFromSource(
        JsonTextReader reader)
    {
        var dto = _deserializer.Deserialize<GroupParamsChangedEventDTO>(reader);

        Debug.Assert(dto is not null);

        return new(
            dto.Id, 
            dto.IsCommited, 
            dto.UserId, 
            dto.GroupId, 
            dto.UpdateMoment, 
            dto.Json);
    }

    private GroupParticipantDeletedEvent CreateParticipantDeletedCreatedFromSource(
        JsonTextReader reader)
    {
        var dto = _deserializer.Deserialize<GroupParticipantDeletedEventDTO>(reader);

        Debug.Assert(dto is not null);

        return new(dto.Id, dto.IsCommited, dto.UserId, dto.GroupId);
    }

    private GroupParticipantInvitedEvent CreateParticipantInvitedFromSource(
        JsonTextReader reader)
    {
        var dto = _deserializer.Deserialize<GroupParticipantInvitedEventDTO>(reader);

        Debug.Assert(dto is not null);

        return new(dto.Id, dto.IsCommited, dto.UserId, dto.GroupId);
    }

    private IssueCreatedEvent CreateIssueCreatedFromSource(
        JsonTextReader reader)
    {
        var dto = _deserializer.Deserialize<IssueCreatedEventDTO>(reader);

        Debug.Assert(dto is not null);

        return new(dto.Id, dto.IsCommited, dto.UserId, dto.IssueId, dto.CreatedMoment);
    }

    private IssueParamsChangedEvent CreateIssueUpdatedFromSource(
        JsonTextReader reader)
    {
        var dto = _deserializer.Deserialize<IssueParamsChangedEventDTO>(reader);

        Debug.Assert(dto is not null);

        return new(
            dto.Id, 
            dto.IsCommited, 
            dto.UserId, 
            dto.IssueId, 
            dto.UpdateMoment, 
            dto.Json);
    }

    private IssueTerminalStatusReceivedEvent CreateIssueTerminatedFromSource(
        JsonTextReader reader)
    {
        var dto = _deserializer.Deserialize<IssueTerminalStatusReceivedEventDTO>(reader);

        Debug.Assert(dto is not null);

        return new(
            dto.Id,
            dto.IsCommited,
            dto.UserId,
            dto.IssueId,
            dto.TerminalMoment,
            dto.TerminalStatus);
    }

    private MeetCreatedEvent CreateMeetCreatedFromSource(
        JsonTextReader reader)
    {
        var dto = _deserializer.Deserialize<MeetCreatedEventDTO>(reader);

        Debug.Assert(dto is not null);

        return new(
            dto.Id,
            dto.IsCommited,
            dto.UserId,
            dto.MeetId,
            dto.CreatedMoment,
            dto.ScheduledStart,
            dto.Duration);
    }

    private MeetGuestDeletedEvent CreateMeetGuestDeletedFromSource(
        JsonTextReader reader)
    {
        var dto = _deserializer.Deserialize<MeetGuestDeletedEventDTO>(reader);

        Debug.Assert(dto is not null);

        return new(dto.Id, dto.IsCommited, dto.UserId, dto.MeetId);
    }

    private MeetGuestInvitedEvent CreateMeetGuestInvitedFromSource(
        JsonTextReader reader)
    {
        var dto = _deserializer.Deserialize<MeetGuestInvitedEventDTO>(reader);

        Debug.Assert(dto is not null);

        return new(dto.Id, dto.IsCommited, dto.UserId, dto.MeetId);
    }

    private MeetParamsChangedEvent CreateMeetUpdatedFromSource(
        JsonTextReader reader)
    {
        var dto = _deserializer.Deserialize<MeetParamsChangedEventDTO>(reader);

        Debug.Assert(dto is not null);

        return new(dto.Id, dto.IsCommited, dto.UserId, dto.MeetId, dto.UpdateMoment, dto.Json);
    }

    private MeetSoonBeginEvent CreateMeetSoonBeginFromSource(
        JsonTextReader reader)
    {
        var dto = _deserializer.Deserialize<MeetSoonBeginEventDTO>(reader);

        Debug.Assert(dto is not null);

        return new(
            dto.Id,
            dto.IsCommited,
            dto.UserId,
            dto.MeetId,
            dto.RemainingTime,
            dto.ScheduledStart);
    }

    private MeetTerminalStatusReceivedEvent CreateMeetTerminatedFromSource(
        JsonTextReader reader)
    {
        var dto = _deserializer.Deserialize<MeetTerminalStatusReceivedEventDTO>(reader);

        Debug.Assert(dto is not null);

        return new(
            dto.Id,
            dto.IsCommited, 
            dto.UserId,
            dto.MeetId,
            dto.TerminalMoment, 
            dto.TerminalStatus);
    }

    private SnapshotCreatedEvent CreateSnapshotCreatedFromSource(
        JsonTextReader reader)
    {
        var dto = _deserializer.Deserialize<SnapshotCreatedEventDTO>(reader);

        Debug.Assert(dto is not null);

        return new(dto.Id, dto.IsCommited, dto.UserId, dto.SnapshotId, dto.CreatedMoment);
    }

    private TaskAssignedEvent CreateTaskAssignedFromSource(
        JsonTextReader reader)
    {
        var dto = _deserializer.Deserialize<TaskAssignedEventDTO>(reader);

        Debug.Assert(dto is not null);

        return new(dto.Id, dto.IsCommited, dto.UserId, dto.TaskId);
    }

    private TaskCreatedEvent CreateTaskCreatedFromSource(
        JsonTextReader reader)
    {
        var dto = _deserializer.Deserialize<TaskCreatedEventDTO>(reader);

        Debug.Assert(dto is not null);

        return new(dto.Id, dto.IsCommited, dto.UserId, dto.TaskId, dto.CreatedMoment);
    }

    private TaskParamsChangedEvent CreateTaskUpdatedFromSource(
        JsonTextReader reader)
    {
        var dto = _deserializer.Deserialize<TaskParamsChangedEventDTO>(reader);

        Debug.Assert(dto is not null);

        return new(
            dto.Id,
            dto.IsCommited, 
            dto.UserId, 
            dto.TaskId, 
            dto.UpdateMoment, 
            dto.Json);
    }

    private TaskTerminalStatusReceivedEvent CreateTaskTerminatedFromSource(
        JsonTextReader reader)
    {
        var dto = _deserializer.Deserialize<TaskTerminalStatusReceivedEventDTO>(reader);

        Debug.Assert(dto is not null);

        return new(
            dto.Id,
            dto.IsCommited,
            dto.UserId,
            dto.TaskId,
            dto.TerminalMoment,
            dto.TerminalStatus);
    }

    private TaskUnassignedEvent CreateTaskUnassignedFromSource(
        JsonTextReader reader)
    {
        var dto = _deserializer.Deserialize<TaskUnassignedEventDTO>(reader);

        Debug.Assert(dto is not null);

        return new(
            dto.Id,
            dto.IsCommited,
            dto.UserId,
            dto.TaskId);
    }

    private UserInfoUpdateEvent CreateUserUpdatedFromSource(
        JsonTextReader reader)
    {
        var dto = _deserializer.Deserialize<UserInfoUpdateEventDTO>(reader);

        Debug.Assert(dto is not null);

        return new(
            dto.Id,
            dto.IsCommited, 
            dto.UserId,
            dto.UpdateMoment,
            dto.Json);
    }

    private UserLoginEvent CreateUserLoginFromSource(
        JsonTextReader reader)
    {
        var dto = _deserializer.Deserialize<UserLoginEventDTO>(reader);

        Debug.Assert(dto is not null);

        return new(
            dto.Id,
            dto.IsCommited,
            dto.UserId,
            dto.FirebaseToken,
            dto.TokenSetMoment);
    }

    private UserLogoutEvent CreateUserLogoutFromSource(
        JsonTextReader reader)
    {
        var dto = _deserializer.Deserialize<UserLogoutEventDTO>(reader);

        Debug.Assert(dto is not null);

        return new(
            dto.Id,
            dto.IsCommited,
            dto.UserId,
            dto.FirebaseToken);
    }

    private UserRegistrationEvent CreateUserRegisterFromSource(
        JsonTextReader reader)
    {
        var dto = _deserializer.Deserialize<UserRegistrationEventDTO>(reader);

        Debug.Assert(dto is not null);

        return new(
            dto.Id,
            dto.IsCommited, 
            dto.UserId,
            dto.FirebaseToken,
            dto.AccountCreationMoment);
    }

    private UserRoleChangedEvent CreateUserRoleChangedFromSource(
        JsonTextReader reader)
    {
        var dto = _deserializer.Deserialize<UserRoleChangedEventDTO>(reader);

        Debug.Assert(dto is not null);

        return new(
            dto.Id, 
            dto.IsCommited,
            dto.UserId,
            dto.UpdateMoment,
            dto.NewRole);
    }

    private static readonly JsonSerializer _deserializer = JsonSerializer.CreateDefault();
}
