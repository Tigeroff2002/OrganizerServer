using Models.Enums;
using Models.RedisEventModels.IssueEvents;

namespace Models.RedisEventModels.MeetEvents;

public sealed record class MeetTerminalStatusReceivedEvent(
    int Id,
    bool IsCommited,
    int UserId,
    int IssueId,
    DateTimeOffset TerminalMoment,
    EventStatus TerminalStatus)
    : MeetBasedEvent(Id, IsCommited, UserId, IssueId);
