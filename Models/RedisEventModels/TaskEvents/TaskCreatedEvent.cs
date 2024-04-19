namespace Models.RedisEventModels.TaskEvents;

public sealed record class TaskCreatedEvent(
    int Id,
    bool IsCommited,
    int UserId, 
    int TaskId,
    DateTimeOffset CreatedMoment)
    : TaskBasedEvent(Id, IsCommited, UserId, TaskId);
