namespace Models.RedisEventModels.UserEvents;

public sealed record class UserInfoUpdateEvent(
    int Id,
    bool IsCommited,
    int UserId,
    DateTimeOffset UpdateMoment,
    string Json)
    : UserRelatedEvent(Id, IsCommited, UserId);
