namespace Models.RedisEventModels;

public record class UserRelatedEvent(string Id, bool IsCommited, int UserId) 
    : BaseEvent(Id, IsCommited);
