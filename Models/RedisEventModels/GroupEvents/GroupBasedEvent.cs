namespace Models.RedisEventModels.GroupEvents;

public abstract record class GroupBasedEvent(
    string Id,
    bool IsCommited,
    int UserId,
    int GroupId)
    : UserRelatedEvent(Id, IsCommited, UserId);
