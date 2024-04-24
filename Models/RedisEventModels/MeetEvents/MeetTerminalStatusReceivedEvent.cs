using Models.Enums;
using Models.RedisEventModels.IssueEvents;

namespace Models.RedisEventModels.MeetEvents;

public sealed record class MeetTerminalStatusReceivedEvent(
    string Id,
    bool IsCommited,
    int UserId,
    int EventId,
    DateTimeOffset TerminalMoment,
    EventStatus TerminalStatus)
    : MeetBasedEvent(Id, IsCommited, UserId, EventId);
