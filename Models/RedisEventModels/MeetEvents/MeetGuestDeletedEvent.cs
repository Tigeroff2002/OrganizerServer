using Models.RedisEventModels.GroupEvents;

namespace Models.RedisEventModels.MeetEvents;

public sealed record class MeetGuestDeletedEvent(
    int Id,
    bool IsCommited,
    int UserId,
    int MeetId)
    : MeetBasedEvent(Id, IsCommited, UserId, MeetId);
