using Models.Enums;
using Models.RedisEventModels.IssueEvents;

namespace Models.RedisEventModels.TaskEvents;

public sealed record class IssueTerminalStatusReceivedEvent(
    int Id,
    bool IsCommited,
    int UserId,
    int TaskId,
    DateTimeOffset TerminalMoment,
    TaskCurrentStatus TerminalStatus)
    : TaskBasedEvent(Id, IsCommited, UserId, TaskId);
