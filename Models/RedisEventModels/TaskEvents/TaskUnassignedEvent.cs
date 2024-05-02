namespace Models.RedisEventModels.TaskEvents;

public sealed record class TaskUnassignedEvent(
    string Id,
    bool IsCommited,
    int UserId,
    int TaskId)
    : TaskBasedEvent(Id, IsCommited, UserId, TaskId);
