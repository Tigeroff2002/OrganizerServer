namespace Models.RedisEventModels.GroupEvents;

public sealed record class GroupParticipantInvitedEvent(
    string Id,
    bool IsCommited,
    int UserId,
    int GroupId)
    : GroupBasedEvent(Id, IsCommited, UserId, GroupId);
