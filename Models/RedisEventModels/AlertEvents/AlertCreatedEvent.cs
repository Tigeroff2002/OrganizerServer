namespace Models.RedisEventModels.AlertEvents;

public sealed record class AlertCreatedEvent(
    string Id,
    bool IsCommited,
    int UserId,
    int AlertId,
    DateTimeOffset CreateMoment,
    string Json) 
    : UserRelatedEvent(Id, IsCommited, UserId);
