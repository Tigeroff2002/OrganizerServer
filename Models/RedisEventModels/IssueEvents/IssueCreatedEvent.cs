using Models.RedisEventModels.GroupEvents;

namespace Models.RedisEventModels.IssueEvents;

public sealed record class IssueCreatedEvent(
    string Id,
    bool IsCommited,
    int UserId,
    int IssueId,
    DateTimeOffset CreatedMoment)
    : IssueBasedEvent(Id, IsCommited, UserId, IssueId);

