namespace Models.RedisEventModels.GroupEvents;

public sealed record class GroupParticipantDeletedEvent(
    int Id,
    bool IsCommited,
    int UserId,
    int GroupId)
    : GroupBasedEvent(Id, IsCommited, UserId, GroupId);
