using Models.RedisEventModels.GroupEvents;

namespace Models.RedisEventModels.IssueEvents;

public sealed record class IssueParamsChangedEvent(
    int Id,
    bool IsCommited,
    int UserId,
    int IssueId,
    DateTimeOffset UpdateMoment,
    string Json)
    : IssueBasedEvent(Id, IsCommited, UserId, IssueId);
