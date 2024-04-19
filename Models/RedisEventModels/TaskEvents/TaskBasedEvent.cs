namespace Models.RedisEventModels.TaskEvents;

public abstract record class TaskBasedEvent(
    int Id,
    bool IsCommited,
    int UserId,
    int TaskId)
    : UserRelatedEvent(Id, IsCommited, UserId);
