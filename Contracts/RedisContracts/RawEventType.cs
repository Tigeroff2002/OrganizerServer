namespace Contracts.RedisContracts;

public enum RawEventType
{
    AlertCreated,

    GroupCreated,

    GroupParamsChanged,

    GroupParticipantDeleted,

    GroupParticipantInvited,

    IssueCreated,

    IssueParamsChanged,

    IssueTerminalStatusReceived,

    MeetCreated,

    MeetGuestDeleted,

    MeetGuestInvited,

    MeetParamsChanged,

    MeetSoonBegin,

    MeetTerminalStatusReceived,

    MeetGuestChangedDecision,

    SnapshotCreated,

    TaskAssignedEvent,

    TaskCreatedEvent,

    TaskParamsChanged,

    TaskTerminalStatusReceived,

    TaskUnassignedEvent,

    UserInfoUpdate,

    UserLogin,

    UserLogout,

    UserRegistration,

    UserRoleChanged
}
