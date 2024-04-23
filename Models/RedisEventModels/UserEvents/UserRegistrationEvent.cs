namespace Models.RedisEventModels.UserEvents;

public sealed record class UserRegistrationEvent(
    string Id,
    bool IsCommited,
    int UserId,
    string FirebaseToken,
    DateTimeOffset AccountCreationMoment) 
    : UserRelatedEvent(Id, IsCommited, UserId);
