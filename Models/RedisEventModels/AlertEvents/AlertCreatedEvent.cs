namespace Models.RedisEventModels.AlertEvents;

public sealed record class AlertCreatedEvent(
    int Id,
    bool IsCommited,
    int UserId,
    int AlertId) 
    : UserRelatedEvent(Id, IsCommited, UserId);
