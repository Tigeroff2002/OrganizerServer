namespace Models.RedisEventModels.GroupEvents;

public sealed record class GroupParamsChangedEvent(
    int Id,
    bool IsCommited,
    int UserId,
    int GroupId,
    DateTimeOffset UpdateMoment,
    string Json)
    : GroupBasedEvent(Id, IsCommited, UserId, GroupId);
