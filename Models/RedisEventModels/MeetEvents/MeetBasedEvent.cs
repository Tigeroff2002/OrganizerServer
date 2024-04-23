namespace Models.RedisEventModels.MeetEvents;

public abstract record class MeetBasedEvent(
    string Id,
    bool IsCommited,
    int UserId,
    int MeetId)
    : UserRelatedEvent(Id, IsCommited, UserId);
