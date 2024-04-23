namespace Models.RedisEventModels.IssueEvents;

public abstract record class IssueBasedEvent(
    string Id,
    bool IsCommited,
    int UserId,
    int IssueId)
    : UserRelatedEvent(Id, IsCommited, UserId);
