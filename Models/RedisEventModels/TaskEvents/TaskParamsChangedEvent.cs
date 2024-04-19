namespace Models.RedisEventModels.TaskEvents;

public sealed record class TaskParamsChangedEvent(
    int Id,
    bool IsCommited,
    int UserId,
    int TaskId,
    DateTimeOffset UpdateMoment,
    string Json)
    : TaskBasedEvent(Id, IsCommited, UserId, TaskId);