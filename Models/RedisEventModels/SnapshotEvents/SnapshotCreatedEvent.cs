namespace Models.RedisEventModels.SnapshotEvents;

public sealed record class SnapshotCreatedEvent(
    int Id,
    bool IsCommited,
    int UserId,
    int SnapshotId)
    : UserRelatedEvent(Id, IsCommited, UserId);
