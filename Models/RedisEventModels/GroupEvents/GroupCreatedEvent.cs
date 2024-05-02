namespace Models.RedisEventModels.GroupEvents;

public sealed record class GroupCreatedEvent(
    string Id, 
    bool IsCommited,
    int UserId,
    int GroupId,
    DateTimeOffset CreatedMoment)
    : GroupBasedEvent(Id, IsCommited, UserId, GroupId);
