namespace Models.RedisEventModels.TaskEvents;

public sealed record class TaskUnassignedEvent(
    int Id,
    bool IsCommited,
    int UserId,
    int TaskId)
    : TaskBasedEvent(Id, IsCommited, UserId, TaskId);
