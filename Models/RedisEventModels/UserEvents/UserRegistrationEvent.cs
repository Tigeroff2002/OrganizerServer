namespace Models.RedisEventModels.UserEvents;

public sealed record class UserRegistrationEvent(
    int Id,
    bool IsCommited,
    int UserId,
    string FirebaseToken,
    DateTimeOffset AccountCreationMoment) 
    : UserRelatedEvent(Id, IsCommited, UserId);
