namespace Models.RedisEventModels;

public record class UserRelatedEvent(int Id, bool IsCommited, int UserId) 
    : BaseEvent(Id, IsCommited);
