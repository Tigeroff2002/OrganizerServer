namespace Models.RedisEventModels.GroupEvents;

public sealed record class GroupParticipantInvitedEvent(
    int Id,
    bool IsCommited,
    int UserId,
    int GroupId)
    : GroupBasedEvent(Id, IsCommited, UserId, GroupId);
