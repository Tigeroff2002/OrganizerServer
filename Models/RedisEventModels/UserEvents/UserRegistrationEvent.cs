namespace Models.RedisEventModels.UserEvents;

public sealed record class UserRegistrationEvent(
    int Id,
    bool IsCommited,
    int UserId,
    DateTimeOffset AccountCreateDate) 
    : UserRelatedEvent(Id, IsCommited, UserId);
