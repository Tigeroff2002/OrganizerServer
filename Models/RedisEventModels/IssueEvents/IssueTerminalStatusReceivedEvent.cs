using Models.Enums;
using Models.RedisEventModels.GroupEvents;

namespace Models.RedisEventModels.IssueEvents;

public sealed record class IssueTerminalStatusReceivedEvent(
    string Id,
    bool IsCommited,
    int UserId,
    int IssueId,
    DateTimeOffset TerminalMoment,
    IssueStatus TerminalStatus)
    : IssueBasedEvent(Id, IsCommited, UserId, IssueId);
