namespace Models.RedisEventModels.UserEvents;

public sealed record class UserLoginEvent(
    int Id,
    bool IsCommited,
    int UserId,
    string FirebaseToken,
    DateTimeOffset TokenSetMoment)
    : UserRelatedEvent(Id, IsCommited, UserId);
