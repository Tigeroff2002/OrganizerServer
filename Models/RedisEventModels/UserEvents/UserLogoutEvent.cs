namespace Models.RedisEventModels.UserEvents;

public sealed record class UserLogoutEvent(
    int Id,
    bool IsCommited,
    int UserId,
    string FirebaseToken)
    : UserRelatedEvent(Id, IsCommited, UserId);
